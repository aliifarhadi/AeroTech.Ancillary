using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated.Backoffice;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Pricings;

[Collection(DatabaseCollection.Name)]
public class V12PricingAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V12PricingAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private async Task<TResult> RequestAsync<TResult>(Func<AncillaryScope, Task<TResult>> request)
    {
        await using var scope = new AncillaryScope(_database, _clock);

        return await request(scope);
    }

    private Task RefusedAsync(int code, int httpStatus, Func<AncillaryScope, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, async () =>
        {
            await using var scope = new AncillaryScope(_database, _clock);

            await request(scope);
        });

    private async Task<(int AirlineId, long SupplierId, long DefinitionId)> DefinitionAsync(
        PricingUnit pricingUnit = PricingUnit.PerPassenger,
        string reference = "SERVICE")
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var definition = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, reference, "SVC", "F", "TS", "Service", pricingUnit: pricingUnit));

        return (airlineId, supplierId, definition.Id);
    }

    private async Task<long> DraftProvisionAsync(
        long definitionId,
        CommercialDisposition disposition = CommercialDisposition.Paid,
        int sequence = 10,
        AncillaryQuantityUnit quantityUnit = AncillaryQuantityUnit.Each,
        PricingUnit pricingUnit = PricingUnit.PerPassenger)
        => (await RequestAsync(scope => scope.DefineProvision.DefineAsync(
            V122Catalog.Fit(Provision(definitionId, sequence, disposition, quantityUnit: quantityUnit), pricingUnit)))).Id;

    private Task<PricingResult> DefinePricingAsync(long provisionId, int currencyId, params PricingLineInput[] priceLines)
        => RequestAsync(scope => scope.DefinePricing.DefineAsync(Pricing(provisionId, currencyId, priceLines)));

    private Task<BackofficePricingDto> PricingAsync(long pricingId)
        => RequestAsync(scope => scope.GetPricingById.ExecuteAsync(pricingId));

    private Task<int> ActiveCountAsync(long provisionId)
        => RequestAsync(scope => scope.Command.AncillaryPricings.AsNoTracking()
            .CountAsync(row => row.AncillaryProvisionId == provisionId && row.Status == PricingStatus.Active));

    private async Task<IReadOnlyList<(long Id, int Version, PricingStatus Status)>> VersionsAsync(long provisionId)
        => (await RequestAsync(scope => scope.Command.AncillaryPricings.AsNoTracking()
                .Where(row => row.AncillaryProvisionId == provisionId)
                .OrderBy(row => row.Version)
                .Select(row => new { row.Id, row.Version, row.Status })
                .ToListAsync()))
            .Select(row => (row.Id, row.Version, row.Status))
            .ToList();

    [Fact]
    public async Task V12_P04_P07_per_passenger_rates_are_filed_by_passenger_type_and_age_band_and_totalled_per_rate_only()
    {
        var (_, _, definitionId) = await DefinitionAsync(reference: "INSURANCE_PLAN");
        var provisionId = await DraftProvisionAsync(definitionId);
        var pricing = await DefinePricingAsync(
            provisionId,
            Eur,
            Base(20m, PassengerTypeCode.ADT, 0, 65),
            Base(40m, PassengerTypeCode.ADT, 65),
            Base(10m, PassengerTypeCode.CHD),
            Base(3.5m, PassengerTypeCode.INF),
            Tax("T1", 2m, PassengerTypeCode.ADT, 0, 65, name: "Filed tax"),
            Tax("T1", 4m, PassengerTypeCode.ADT, 65),
            Fee("SV", 1.25m, PassengerTypeCode.CHD));
        var detail = await PricingAsync(pricing.Id);

        Assert.Equal(
            ("PerPassenger", Eur, "EUR", "Item", "Draft", 1),
            (detail.PricingUnit!.Name, detail.CurrencyId, detail.Currency, detail.Rates.SelectMany(rate => rate.Components).Single(component => component.Category.Name == "Fee").FeeApplicationUnit!.Name, detail.Status.Name, detail.Version));
        Assert.Equal(7, detail.PriceLines.Count);
        Assert.Equal(7, detail.PriceLines.Select(line => line.Id).Distinct().Count());
        Assert.Equal(
            new[]
            {
                ("ADT", (int?)0, (int?)65, 20m, 2m, 0m, 22m),
                ("ADT", 65, null, 40m, 4m, 0m, 44m),
                ("CHD", null, null, 10m, 0m, 1.25m, 11.25m),
                ("INF", null, null, 3.5m, 0m, 0m, 3.5m)
            },
            detail.Rates.Select(rate => (rate.PassengerTypeCode!.Name, rate.AgeFromInclusive, rate.AgeToExclusive, rate.BaseAmount, rate.TaxAmount, rate.FeeAmount, rate.TotalAmount)));
        Assert.Equal(80.75m, detail.PriceLines.Sum(line => line.Amount));
        Assert.DoesNotContain(detail.Rates, rate => rate.TotalAmount == 80.75m);

        var listed = await RequestAsync(scope => scope.GetPricingsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryPricingsPaginatedQuery { AncillaryProvisionId = provisionId }));

        Assert.Equal((pricing.Id.ToString(), 4, "EUR"), listed.Results.Select(row => (row.Id, row.RateCount, row.Currency)).Single());

        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(
            Pricing(provisionId, Eur, Base(20m, PassengerTypeCode.ADT, 0, 65), Base(40m, PassengerTypeCode.ADT, 64))));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(
            Pricing(provisionId, Eur, Base(20m, PassengerTypeCode.ADT, 0, 65), Base(21m, PassengerTypeCode.ADT, 0, 65))));
        await RefusedAsync(16508, 409, scope => scope.ChangePricing.ChangeAsync(
            Change(pricing.Id, Pricing(provisionId, Eur, Base(20m, PassengerTypeCode.ADT), Base(40m, PassengerTypeCode.ADT, 65)))));
        await RefusedAsync(16508, 409, scope => scope.ChangePricing.ChangeAsync(
            Change(pricing.Id, Pricing(provisionId, Eur, Base(20m), Base(10m, PassengerTypeCode.CHD)))));
        await RefusedAsync(16502, 422, scope => scope.ChangePricing.ChangeAsync(
            Change(pricing.Id, Pricing(provisionId, Eur, Base(20m, PassengerTypeCode.ADT, 65, 65)))));
        await RefusedAsync(16502, 422, scope => scope.ChangePricing.ChangeAsync(
            Change(pricing.Id, Pricing(provisionId, Eur, Base(20m, PassengerTypeCode.ADT, null, 65)))));
        await RefusedAsync(16502, 422, scope => scope.ChangePricing.ChangeAsync(
            Change(pricing.Id, Pricing(provisionId, Eur, Base(-20m, PassengerTypeCode.ADT)))));
        await RefusedAsync(16502, 422, scope => scope.ChangePricing.ChangeAsync(
            Change(pricing.Id, Pricing(provisionId, Eur, Base(0m, PassengerTypeCode.ADT)))));
        await RefusedAsync(16512, 422, scope => scope.ChangePricing.ChangeAsync(
            Change(pricing.Id, Pricing(provisionId, Eur, Base(20.005m, PassengerTypeCode.ADT)))));

        Assert.Equal(detail.PriceLines, (await PricingAsync(pricing.Id)).PriceLines);

        var amountColumns = await RequestAsync(scope => scope.Command.Database
            .SqlQueryRaw<string>(
                "SELECT s.name + '.' + t.name + ':' + CAST(c.precision AS varchar(5)) + ',' + CAST(c.scale AS varchar(5)) AS Value " +
                "FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id JOIN sys.schemas s ON s.schema_id = t.schema_id " +
                "WHERE t.name IN ('AncillaryPricingLines', 'AncillaryPricingRates') AND c.name IN ('Amount', 'BaseAmount') ORDER BY s.name")
            .ToListAsync());

        Assert.Equal(new[] { "Ancillary.AncillaryPricingRates:19,6", "ReadModel.AncillaryPricingRates:19,6" }, amountColumns);
    }

    [Theory]
    [InlineData(PricingUnit.PerItem, "WIFI_ITEM")]
    [InlineData(PricingUnit.PerSeat, "SEAT_CHOICE")]
    [InlineData(PricingUnit.PerPiece, "EXTRA_PIECE")]
    [InlineData(PricingUnit.PerKilogram, "EXTRA_KILO")]
    public async Task V12_P05_a_non_passenger_unit_files_one_rate_in_one_currency_and_refuses_passenger_selectors(PricingUnit pricingUnit, string reference)
    {
        var (_, _, definitionId) = await DefinitionAsync(pricingUnit, reference);
        var provisionId = await DraftProvisionAsync(
            definitionId,
            quantityUnit: pricingUnit switch
            {
                PricingUnit.PerPiece => AncillaryQuantityUnit.Piece,
                PricingUnit.PerKilogram => AncillaryQuantityUnit.Kilogram,
                _ => AncillaryQuantityUnit.Each
            },
            pricingUnit: pricingUnit);

        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(Pricing(provisionId, Eur, Base(45m, PassengerTypeCode.ADT))));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(Pricing(provisionId, Eur, Base(45m, null, 0, 65), Base(50m, null, 65))));
        await RefusedAsync(16502, 422, scope => scope.DefinePricing.DefineAsync(Pricing(provisionId, 0, Base(45m))));

        var pricing = await DefinePricingAsync(provisionId, Usd, Base(45m), Tax("VAT", 4.5m));
        var published = await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, pricing.Id)));
        var detail = await PricingAsync(pricing.Id);
        var rate = Assert.Single(detail.Rates);

        Assert.Equal((pricingUnit, ProvisionStatus.Active), (pricing.PricingUnit!.Value, published.Status));
        Assert.Equal((pricingUnit.ToString(), Usd, "USD", "Active"), (detail.PricingUnit!.Name, detail.CurrencyId, detail.Currency, detail.Status.Name));
        Assert.Null(rate.PassengerTypeCode);
        Assert.Null(rate.AgeFromInclusive);
        Assert.Null(rate.AgeToExclusive);
        Assert.Equal((45m, 4.5m, 0m, 49.5m), (rate.BaseAmount, rate.TaxAmount, rate.FeeAmount, rate.TotalAmount));
        Assert.All(detail.PriceLines, line => Assert.Null(line.PassengerTypeCode));
        Assert.Equal(1, await ActiveCountAsync(provisionId));
    }

    [Fact]
    public async Task V12_P06_tax_and_fee_components_round_trip_with_category_code_name_country_and_station()
    {
        var (_, _, definitionId) = await DefinitionAsync(PricingUnit.PerItem, "LOUNGE_PASS");
        var provisionId = await DraftProvisionAsync(definitionId);

        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(new TestDefinePricingCommand(provisionId, Eur, FeeApplicationUnit.Item, [])));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(Pricing(provisionId, Eur, Base(25m), Base(5m))));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(
            Pricing(provisionId, Eur, Base(25m), Tax("VAT", 2m, countryId: 1), Tax("VAT", 3m, countryId: 1))));
        await RefusedAsync(16502, 422, scope => scope.DefinePricing.DefineAsync(Pricing(provisionId, Eur, Base(25m), Tax(null, 2m))));
        await RefusedAsync(16502, 422, scope => scope.DefinePricing.DefineAsync(Pricing(provisionId, Eur, Base(25m), Fee(null, 2m))));
        await RefusedAsync(16502, 422, scope => scope.DefinePricing.DefineAsync(Pricing(provisionId, Eur, Base(25m), Tax("VAT", -1m))));

        var pricing = await DefinePricingAsync(
            provisionId,
            Eur,
            Base(25m, name: "Lounge access"),
            Tax("VAT", 2.5m, countryId: 1, name: "Value added tax"),
            Tax("VAT", 0.5m, countryId: 2, name: "Value added tax"),
            Tax("APT", 1.5m, stationAirportId: Ika, name: "Airport charge"),
            Fee("SVC", 0.75m),
            Fee("WVR", 0m));

        await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, pricing.Id)));

        var detail = await PricingAsync(pricing.Id);
        var rate = Assert.Single(detail.Rates);

        Assert.Equal(
            new[]
            {
                ("Ancillary", (string?)null, (string?)null, (int?)null, (int?)null, 25m),
                ("Tax", "VAT", "Value added tax", 1, null, 2.5m),
                ("Tax", "VAT", "Value added tax", 2, null, 0.5m),
                ("Tax", "APT", "Airport charge", null, Ika, 1.5m),
                ("Fee", "SVC", null, null, null, 0.75m),
                ("Fee", "WVR", null, null, null, 0m)
            },
            detail.PriceLines.Select(line => (line.Category.Name, line.Code, line.Name, line.CountryId, line.StationAirportId, line.Amount)));
        Assert.Equal((25m, 4.5m, 0.75m, 30.25m), (rate.BaseAmount, rate.TaxAmount, rate.FeeAmount, rate.TotalAmount));

        var readModelLines = await RequestAsync(async scope =>
            (await scope.Query.AncillaryPricingRates.AsNoTracking()
                .Where(rate => rate.AncillaryPricingId == pricing.Id)
                .Select(rate => new { rate.Id, Amount = rate.BaseAmount })
                .ToListAsync())
            .Concat(await scope.Query.AncillaryPriceComponents.AsNoTracking()
                .Where(component => component.AncillaryPricingId == pricing.Id)
                .Select(component => new { component.Id, component.Amount })
                .ToListAsync())
            .OrderBy(line => line.Id)
            .ToList());
        var commandLines = await RequestAsync(async scope =>
            (await scope.Command.Set<AncillaryPricingRate>().AsNoTracking()
                .Where(rate => rate.AncillaryPricingId == pricing.Id)
                .Select(rate => new { rate.Id, Amount = rate.BaseAmount })
                .ToListAsync())
            .Concat(await scope.Command.Set<AncillaryPricingRate>().AsNoTracking()
                .Where(rate => rate.AncillaryPricingId == pricing.Id)
                .SelectMany(rate => rate.Components)
                .Select(component => new { component.Id, component.Amount.Amount })
                .ToListAsync())
            .OrderBy(line => line.Id)
            .ToList());

        Assert.Equal(detail.PriceLines.Select(line => (line.Id, line.Amount)), readModelLines.Select(line => (line.Id, line.Amount)));
        Assert.Equal(detail.PriceLines.Select(line => (line.Id, line.Amount)), commandLines.Select(line => (line.Id, line.Amount)));

        await RefusedAsync(16503, 409, scope => scope.ChangePricing.ChangeAsync(Change(pricing.Id, Pricing(provisionId, Eur, Base(99m)))));
        Assert.Equal(detail.PriceLines, (await PricingAsync(pricing.Id)).PriceLines);
    }
}
