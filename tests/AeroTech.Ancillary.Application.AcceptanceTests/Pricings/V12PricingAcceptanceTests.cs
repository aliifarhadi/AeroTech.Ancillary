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
        AncillaryQuantityUnit quantityUnit = AncillaryQuantityUnit.Each)
        => (await RequestAsync(scope => scope.DefineProvision.DefineAsync(Provision(definitionId, sequence, disposition, quantityUnit: quantityUnit)))).Id;

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
    public async Task V12_P01_the_pricing_unit_is_fixed_for_a_service_identity_once_a_version_is_published()
    {
        var (airlineId, supplierId, _) = await DefinitionAsync();
        var command = CarrierDefinition(airlineId, supplierId, "SIM_CARD", "SIM", "M", "ST", "Travel SIM card");
        var first = await RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(command));

        Assert.Equal(PricingUnit.PerPassenger, first.PricingUnit);

        var changed = await RequestAsync(scope => scope.ChangeServiceDefinition.ChangeAsync(
            Change(first.Id, command with { PricingUnit = PricingUnit.PerItem })));

        Assert.Equal(PricingUnit.PerItem, changed.PricingUnit);

        var otherDraft = await RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(command with { PricingUnit = PricingUnit.PerVehicle }));

        Assert.Equal((2, PricingUnit.PerVehicle), (otherDraft.Version, otherDraft.PricingUnit!.Value));

        await RequestAsync(scope => scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(first.Id)));

        await RefusedAsync(16203, 409, scope => scope.ChangeServiceDefinition.ChangeAsync(
            Change(first.Id, command with { PricingUnit = PricingUnit.PerItem, CommercialName = "Renamed" })));
        await RefusedAsync(16209, 409, scope => scope.ChangeServiceDefinition.ChangeAsync(
            Change(first.Id, command with { PricingUnit = PricingUnit.PerPassenger })));
        await RefusedAsync(16209, 409, scope => scope.DefineServiceDefinition.DefineAsync(command with { PricingUnit = PricingUnit.PerRoom }));
        await RefusedAsync(16209, 409, scope => scope.ChangeServiceDefinition.ChangeAsync(
            Change(otherDraft.Id, command with { PricingUnit = PricingUnit.PerVehicle })));

        var aligned = await RequestAsync(scope => scope.ChangeServiceDefinition.ChangeAsync(
            Change(otherDraft.Id, command with { PricingUnit = PricingUnit.PerItem })));
        var revision = await RequestAsync(scope => scope.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(first.Id)));

        Assert.Equal((PricingUnit.PerItem, PricingUnit.PerItem, 3), (aligned.PricingUnit!.Value, revision.PricingUnit!.Value, revision.Version));
        await RefusedAsync(16209, 409, scope => scope.ChangeServiceDefinition.ChangeAsync(
            Change(revision.Id, command with { PricingUnit = PricingUnit.PerSeat })));
        await RefusedAsync(16211, 409, scope => scope.AssignPricingUnit.AssignAsync(new TestAssignPricingUnitCommand(first.Id, PricingUnit.PerItem)));
        await RefusedAsync(16209, 409, scope => scope.AssignPricingUnit.AssignAsync(new TestAssignPricingUnitCommand(first.Id, PricingUnit.PerRoom)));

        var otherAirline = _database.NextAirlineId();
        var otherSupplier = await _proof.SupplierAsync(M1Commands.LocalSupplier(otherAirline));
        var sameReference = await RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(
            command with { OwnerAirlineId = otherAirline, SupplierId = otherSupplier, PricingUnit = PricingUnit.PerPassenger }));

        Assert.Equal((1, PricingUnit.PerPassenger), (sameReference.Version, sameReference.PricingUnit!.Value));

        var detail = await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(revision.Id));
        var stored = await RequestAsync(scope => scope.Command.AncillaryServiceDefinitions.AsNoTracking()
            .Where(row => row.OwnerAirlineId == airlineId && row.ServiceDefinitionRef == "SIM_CARD")
            .OrderBy(row => row.Version)
            .Select(row => row.PricingUnit)
            .ToListAsync());

        Assert.Equal((3, "PerItem", "Per Item"), (detail.Version, detail.PricingUnit!.Name, detail.PricingUnit.Title));
        Assert.Equal(new PricingUnit?[] { PricingUnit.PerItem, PricingUnit.PerItem, PricingUnit.PerItem }, stored);
        Assert.DoesNotContain(typeof(IDefineAncillaryPricingCommand).GetProperties(), property => property.Name == nameof(PricingResult.PricingUnit));
    }

    [Fact]
    public async Task V12_P01_a_pricing_takes_the_unit_of_its_service_and_a_wrong_unit_is_never_published()
    {
        var (airlineId, supplierId, _) = await DefinitionAsync();
        var command = CarrierDefinition(airlineId, supplierId, "TRANSFER", "TRF", "F", "GT", "Airport transfer");
        var definition = await RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(command));
        var provisionId = await DraftProvisionAsync(definition.Id);
        var perPassenger = await DefinePricingAsync(provisionId, Eur, Base(18m, PassengerTypeCode.ADT), Base(9m, PassengerTypeCode.CHD));

        Assert.Equal(PricingUnit.PerPassenger, perPassenger.PricingUnit);

        await RequestAsync(scope => scope.ChangeServiceDefinition.ChangeAsync(
            Change(definition.Id, command with { PricingUnit = PricingUnit.PerVehicle })));

        await RefusedAsync(16506, 409, scope => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(perPassenger.Id)));
        await RefusedAsync(16307, 409, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, perPassenger.Id)));

        await RequestAsync(scope => scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definition.Id)));

        await RefusedAsync(16506, 409, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, perPassenger.Id)));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(Pricing(provisionId, Eur, Base(18m, PassengerTypeCode.ADT))));

        var perVehicle = await DefinePricingAsync(provisionId, Eur, Base(80m));
        var published = await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, perVehicle.Id)));

        Assert.Equal((PricingUnit.PerVehicle, 2, ProvisionStatus.Active), (perVehicle.PricingUnit!.Value, perVehicle.Version, published.Status));
        Assert.Equal(
            new[] { (perPassenger.Id, 1, PricingStatus.Draft), (perVehicle.Id, 2, PricingStatus.Active) },
            await VersionsAsync(provisionId));
    }

    [Fact]
    public async Task V12_P02_a_provision_owns_many_price_versions_and_only_one_is_active_even_under_a_race()
    {
        var (_, _, definitionId) = await DefinitionAsync();
        var provisionId = await DraftProvisionAsync(definitionId);
        var first = (await DefinePricingAsync(provisionId, Eur, Base(20m))).Id;
        var second = (await DefinePricingAsync(provisionId, Eur, Base(22m))).Id;
        var third = (await DefinePricingAsync(provisionId, Eur, Base(24m))).Id;
        var fourth = (await DefinePricingAsync(provisionId, Eur, Base(26m))).Id;
        var activated = await RequestAsync(scope => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(first)));

        Assert.Equal((PricingStatus.Active, 1), (activated.Status, activated.Version));
        await RefusedAsync(16507, 409, scope => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(second)));

        await RequestAsync(scope => scope.SuspendPricing.SuspendAsync(new TestPricingLifecycleCommand(first)));
        await RequestAsync(scope => scope.RetirePricing.RetireAsync(new TestPricingLifecycleCommand(fourth)));

        Assert.Equal(0, await ActiveCountAsync(provisionId));
        await RefusedAsync(16503, 409, scope => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(fourth)));

        await using (var loser = new AncillaryScope(_database, _clock))
        {
            var losing = (await loser.Pricings.GetAsync(third))!;

            Assert.Null(await loser.Pricings.FindActiveAsync(provisionId));
            losing.Activate(Scales, _clock.Now);

            await RequestAsync(scope => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(second)));

            await BusinessAssert.ThrowsAsync(16507, 409, () => loser.UnitOfWork.SaveChangesAsync());
        }

        Assert.Equal(
            new[]
            {
                (first, 1, PricingStatus.Suspended), (second, 2, PricingStatus.Active), (third, 3, PricingStatus.Draft),
                (fourth, 4, PricingStatus.Retired)
            },
            await VersionsAsync(provisionId));
        await RefusedAsync(16507, 409, scope => scope.ReactivatePricing.ReactivateAsync(new TestPricingLifecycleCommand(first)));
        Assert.Equal(1, await ActiveCountAsync(provisionId));

        var indexes = await RequestAsync(scope => scope.Command.Database
            .SqlQueryRaw<string>(
                "SELECT i.name + '|' + ISNULL(i.filter_definition, '') AS Value FROM sys.indexes i " +
                "WHERE i.object_id = OBJECT_ID('Ancillary.AncillaryPricings') AND i.is_unique = 1 AND i.is_primary_key = 0 ORDER BY i.name")
            .ToListAsync());

        Assert.Equal(
            new[] { "IX_AncillaryPricings_AncillaryProvisionId_Version|", "IX_AncillaryPricings_OneActivePerProvision|([Status]=(2))" },
            indexes);

        var listed = await RequestAsync(scope => scope.GetPricingsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryPricingsPaginatedQuery { AncillaryProvisionId = provisionId }));

        Assert.Equal(
            new[] { (1, "Suspended"), (2, "Active"), (3, "Draft"), (4, "Retired") },
            listed.Results.Select(row => (row.Version, row.Status.Name)).OrderBy(row => row.Version));
    }

    [Fact]
    public async Task V12_P03_an_active_paid_provision_has_exactly_one_active_price_and_a_free_or_unavailable_one_has_none()
    {
        var (_, _, definitionId) = await DefinitionAsync();
        var paid = await DraftProvisionAsync(definitionId);
        var free = await DraftProvisionAsync(definitionId, CommercialDisposition.Free, 20);
        var notAvailable = await DraftProvisionAsync(definitionId, CommercialDisposition.NotAvailable, 30);

        await RefusedAsync(16310, 409, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(paid)));

        var price = (await DefinePricingAsync(paid, Eur, Base(25m))).Id;

        await RefusedAsync(16310, 409, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(paid)));
        await RefusedAsync(16505, 409, scope => scope.DefinePricing.DefineAsync(Pricing(free, Eur, Base(25m))));
        await RefusedAsync(16505, 409, scope => scope.DefinePricing.DefineAsync(Pricing(notAvailable, Eur, Base(25m))));
        await RefusedAsync(16504, 422, scope => scope.DefinePricing.DefineAsync(Pricing(987_654_321, Eur, Base(25m))));

        await RequestAsync(scope => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(price)));

        Assert.Equal(
            (ProvisionStatus.Active, ProvisionStatus.Active, ProvisionStatus.Active),
            ((await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(paid)))).Status,
                (await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(free)))).Status,
                (await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(notAvailable)))).Status));
        Assert.Equal((1, 0, 0), (await ActiveCountAsync(paid), await ActiveCountAsync(free), await ActiveCountAsync(notAvailable)));

        await RefusedAsync(16509, 409, scope => scope.SuspendPricing.SuspendAsync(new TestPricingLifecycleCommand(price)));
        await RefusedAsync(16509, 409, scope => scope.RetirePricing.RetireAsync(new TestPricingLifecycleCommand(price)));
        Assert.Equal(1, await ActiveCountAsync(paid));
        Assert.Equal("Active", (await PricingAsync(price)).Status.Name);

        await RequestAsync(scope => scope.SuspendProvision.SuspendAsync(new TestProvisionLifecycleCommand(paid)));
        await RequestAsync(scope => scope.SuspendPricing.SuspendAsync(new TestPricingLifecycleCommand(price)));

        await RefusedAsync(16310, 409, scope => scope.ReactivateProvision.ReactivateAsync(new TestProvisionLifecycleCommand(paid)));
        Assert.Equal("Suspended", (await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(paid))).Status.Name);

        await RequestAsync(scope => scope.ReactivatePricing.ReactivateAsync(new TestPricingLifecycleCommand(price)));

        Assert.Equal(
            ProvisionStatus.Active,
            (await RequestAsync(scope => scope.ReactivateProvision.ReactivateAsync(new TestProvisionLifecycleCommand(paid)))).Status);

        var turnedFree = await DraftProvisionAsync(definitionId, sequence: 40);
        var orphanPrice = (await DefinePricingAsync(turnedFree, Eur, Base(5m))).Id;

        await RequestAsync(scope => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(orphanPrice)));
        await RequestAsync(scope => scope.ChangeProvision.ChangeAsync(Change(turnedFree, Provision(definitionId, 40, CommercialDisposition.Free))));

        await RefusedAsync(16311, 409, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(turnedFree)));
        await RefusedAsync(16505, 409, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(turnedFree, orphanPrice)));

        await RequestAsync(scope => scope.RetirePricing.RetireAsync(new TestPricingLifecycleCommand(orphanPrice)));

        Assert.Equal(
            ProvisionStatus.Active,
            (await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(turnedFree)))).Status);
        Assert.Equal(0, await ActiveCountAsync(turnedFree));
    }

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
    [InlineData(PricingUnit.PerRoom, "HOTEL_ROOM")]
    [InlineData(PricingUnit.PerItem, "SIM_ITEM")]
    [InlineData(PricingUnit.PerVehicle, "PRIVATE_TRANSFER")]
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
            });

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

    [Fact]
    public async Task V12_P08_the_active_price_is_switched_atomically_and_a_stale_expectation_conflicts()
    {
        var (_, _, definitionId) = await DefinitionAsync(PricingUnit.PerItem, "PRIORITY");
        var provisionId = await DraftProvisionAsync(definitionId);
        var otherProvisionId = await DraftProvisionAsync(definitionId, sequence: 20);
        var first = (await DefinePricingAsync(provisionId, Eur, Base(10m), Tax("VAT", 1m))).Id;

        await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, first)));

        var published = _clock.Now;
        var second = (await RequestAsync(scope => scope.RevisePricing.ReviseAsync(new TestPricingLifecycleCommand(first)))).Id;
        var revision = await PricingAsync(second);

        Assert.Equal(
            ("Draft", 2, 10m, 1m),
            (revision.Status.Name, revision.Version, revision.Rates.Single().BaseAmount, revision.Rates.Single().TaxAmount));

        await RequestAsync(scope => scope.ChangePricing.ChangeAsync(Change(second, Pricing(provisionId, Eur, Base(12m), Tax("VAT", 1.2m)))));

        var third = (await DefinePricingAsync(provisionId, Eur, Base(14m))).Id;
        var foreign = (await DefinePricingAsync(otherProvisionId, Eur, Base(50m))).Id;

        _clock.Now = _clock.Now.AddHours(1);

        await RefusedAsync(16510, 409, scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(provisionId, third, second)));
        await RefusedAsync(16501, 404, scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(provisionId, foreign, first)));
        await RefusedAsync(16507, 409, scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(provisionId, first, first)));
        Assert.Equal(
            new[] { PricingStatus.Active, PricingStatus.Draft, PricingStatus.Draft },
            (await VersionsAsync(provisionId)).Select(row => row.Status));

        var switched = await RequestAsync(scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(provisionId, third, first)));
        var retired = await PricingAsync(first);
        var current = await PricingAsync(third);

        Assert.Equal((third, PricingStatus.Active, 3), (switched.Id, switched.Status, switched.Version));
        Assert.Equal(("Retired", published, _clock.Now), (retired.Status.Name, retired.ActivatedAt!.Value, retired.RetiredAt!.Value));
        Assert.Equal(("Active", _clock.Now), (current.Status.Name, current.ActivatedAt!.Value));
        Assert.Equal((10m, 1m, 11m), retired.Rates.Select(rate => (rate.BaseAmount, rate.TaxAmount, rate.TotalAmount)).Single());
        Assert.Equal(1, await ActiveCountAsync(provisionId));

        await RefusedAsync(16510, 409, scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(provisionId, second, first)));

        var switchedBack = await RequestAsync(scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(provisionId, second)));

        Assert.True(first < third && second < third);
        Assert.Equal((second, PricingStatus.Active), (switchedBack.Id, switchedBack.Status));
        Assert.Equal(1, await ActiveCountAsync(provisionId));
        await RefusedAsync(16503, 409, scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(provisionId, first, second)));
        Assert.Equal(
            new[] { PricingStatus.Retired, PricingStatus.Active, PricingStatus.Retired },
            (await VersionsAsync(provisionId)).Select(row => row.Status));

        var fourth = (await DefinePricingAsync(provisionId, Eur, Base(16m))).Id;
        var fifth = (await DefinePricingAsync(provisionId, Eur, Base(18m))).Id;

        await using (var loser = new AncillaryScope(_database, _clock))
        {
            var loserOld = (await loser.Pricings.FindActiveAsync(provisionId))!;
            var loserNew = (await loser.Pricings.GetAsync(fifth))!;

            Assert.Equal(second, loserOld.Id);
            loserOld.Supersede(_clock.Now);
            loserNew.Activate(Scales, _clock.Now);

            await RequestAsync(scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(provisionId, fourth, second)));

            var conflict = await Assert.ThrowsAsync<BusinessException>(() => loser.UnitOfWork.SaveChangesAsync());

            Assert.Contains(conflict.Code, new[] { 16005, 16507 });
            Assert.Equal(409, conflict.HttpStatus);
        }

        Assert.Equal(
            new[]
            {
                (first, 1, PricingStatus.Retired), (second, 2, PricingStatus.Retired), (third, 3, PricingStatus.Retired),
                (fourth, 4, PricingStatus.Active), (fifth, 5, PricingStatus.Draft)
            },
            await VersionsAsync(provisionId));
        Assert.Equal((12m, 1.2m), (await PricingAsync(second)).Rates.Select(rate => (rate.BaseAmount, rate.TaxAmount)).Single());
        Assert.Equal("Active", (await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(provisionId))).Status.Name);
        Assert.Equal(
            new[] { ("Active", 4) },
            (await RequestAsync(scope => scope.GetPricingsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryPricingsPaginatedQuery { AncillaryProvisionId = provisionId, Status = PricingStatus.Active })))
            .Results.Select(row => (row.Status.Name, row.Version)));
        Assert.Equal("Draft", (await PricingAsync(foreign)).Status.Name);
    }
}
