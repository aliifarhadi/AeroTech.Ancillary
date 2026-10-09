using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Pricings;

[Collection(DatabaseCollection.Name)]
public class V121FinalPricingAcceptanceTests
{
    private const int Jpy = 75;
    private const int Kwd = 82;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V121FinalPricingAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private static PricingRateInput Rate(
        decimal amount,
        int currencyId,
        PassengerTypeCode? passengerTypeCode = null,
        params PriceComponentInput[] components)
        => new(passengerTypeCode, null, null, new MoneyInput(amount, currencyId), components);

    private static PriceComponentInput Component(
        AncillaryPriceLineCategory category,
        string code,
        decimal amount,
        int currencyId,
        FeeApplicationUnit? unit = null)
        => new(category, code, null, null, null, new MoneyInput(amount, currencyId), unit, null);

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

    private async Task<long> DraftProvisionAsync(PricingUnit pricingUnit, CommercialDisposition disposition = CommercialDisposition.Paid)
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var definition = await _proof.DefinitionAsync(CarrierDefinition(airlineId, supplierId, "SERVICE", "SVC", "F", "TS", "Service", pricingUnit: pricingUnit));

        return (await RequestAsync(scope => scope.DefineProvision.DefineAsync(Provision(definition.Id, 10, disposition)))).Id;
    }

    private Task<PricingResult> DefineAsync(long provisionId, params PricingRateInput[] rates)
        => RequestAsync(scope => scope.DefinePricing.DefineAsync(new TestDefinePricingRatesCommand(provisionId, rates)));

    private Task<List<string>> RowsAsync(FormattableString sql)
        => RequestAsync(scope => scope.Command.Database.SqlQuery<string>(sql).ToListAsync());

    [Fact]
    public async Task PR01_PR02_one_active_price_keeps_every_currency_as_its_own_authored_rate_without_a_root_currency_or_a_conversion()
    {
        var provisionId = await DraftProvisionAsync(PricingUnit.PerPassenger);
        var defined = await DefineAsync(
            provisionId,
            Rate(40m, Eur, PassengerTypeCode.ADT, Component(AncillaryPriceLineCategory.Tax, "VAT", 4m, Eur), Component(AncillaryPriceLineCategory.Fee, "SVC", 1m, Eur, FeeApplicationUnit.Item)),
            Rate(20m, Eur, PassengerTypeCode.CHD),
            Rate(44m, Usd, PassengerTypeCode.ADT),
            Rate(21.5m, Usd, PassengerTypeCode.CHD));

        await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, defined.Id)));

        var pricing = await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(defined.Id));

        Assert.Equal(new[] { Eur, Usd }, defined.CurrencyIds.Order());
        Assert.Equal("Active", pricing.Status.Name);
        Assert.Equal(new[] { "EUR", "USD" }, pricing.Currencies.Order());
        Assert.Equal(
            new[] { "ADT 40 EUR 45 EUR", "ADT 44 USD 44 USD", "CHD 20 EUR 20 EUR", "CHD 21.5 USD 21.5 USD" },
            pricing.Rates
                .Select(rate => FormattableString.Invariant($"{rate.PassengerTypeCode!.Name} {rate.BasePrice.Amount:0.##} {rate.BasePrice.Currency} {rate.UnitTotal.Amount:0.##} {rate.UnitTotal.Currency}"))
                .Order(StringComparer.Ordinal));
        Assert.All(pricing.Rates.SelectMany(rate => rate.Components), component => Assert.Equal("EUR", component.Amount.Currency));
        Assert.Equal(
            new[] { "Ancillary 1 4 2", "ReadModel 1 4 2" },
            await RowsAsync($"""
                SELECT CONCAT('Ancillary ', (SELECT COUNT(*) FROM Ancillary.AncillaryPricings WHERE AncillaryProvisionId = {provisionId} AND Status = 2), ' ',
                    (SELECT COUNT(*) FROM Ancillary.AncillaryPricingRates WHERE AncillaryPricingId = {defined.Id}), ' ',
                    (SELECT COUNT(DISTINCT CurrencyId) FROM Ancillary.AncillaryPricingRates WHERE AncillaryPricingId = {defined.Id})) AS Value
                UNION ALL
                SELECT CONCAT('ReadModel ', (SELECT COUNT(*) FROM ReadModel.AncillaryPricings WHERE AncillaryProvisionId = {provisionId} AND Status = 2), ' ',
                    (SELECT COUNT(*) FROM ReadModel.AncillaryPricingRates WHERE AncillaryPricingId = {defined.Id}), ' ',
                    (SELECT COUNT(DISTINCT CurrencyId) FROM ReadModel.AncillaryPricingRates WHERE AncillaryPricingId = {defined.Id}))
                """));
        Assert.Empty(await RowsAsync($"""
            SELECT CONCAT(s.name, '.', t.name, '.', c.name) AS Value
            FROM sys.columns AS c JOIN sys.tables AS t ON t.object_id = c.object_id JOIN sys.schemas AS s ON s.schema_id = t.schema_id
            WHERE s.name IN ('Ancillary', 'ReadModel')
              AND ((t.name = 'AncillaryPricings' AND c.name IN ('CurrencyId', 'FeeApplicationUnit'))
                OR (t.name LIKE 'Ancillary%Pric%' AND (c.name LIKE '%Exchange%' OR c.name LIKE '%Fx%' OR c.name LIKE '%PointOfSale%')))
            """));
    }

    [Fact]
    public async Task PR04_PR05_PR06_PR07_amounts_of_zero_two_and_three_decimal_currencies_are_stored_exactly_and_a_finer_scale_is_refused()
    {
        var provisionId = await DraftProvisionAsync(PricingUnit.PerItem);
        var defined = await DefineAsync(provisionId, Rate(1201m, Jpy), Rate(12.34m, Eur, null, Component(AncillaryPriceLineCategory.Tax, "VAT", 1.11m, Eur)), Rate(12.125m, Kwd));

        await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, defined.Id)));

        Assert.Equal(
            new[] { "Ancillary 47 12.340000 1.110000", "Ancillary 75 1201.000000 ", "Ancillary 82 12.125000 ", "ReadModel 47 12.340000 1.110000", "ReadModel 75 1201.000000 ", "ReadModel 82 12.125000 " },
            await RowsAsync($"""
                SELECT CONCAT('Ancillary ', rate.CurrencyId, ' ', rate.BaseAmount, ' ', (SELECT component.Amount FROM Ancillary.AncillaryPriceComponents AS component WHERE component.AncillaryPricingRateId = rate.Id)) AS Value
                FROM Ancillary.AncillaryPricingRates AS rate WHERE rate.AncillaryPricingId = {defined.Id}
                UNION ALL
                SELECT CONCAT('ReadModel ', rate.CurrencyId, ' ', rate.BaseAmount, ' ', (SELECT component.Amount FROM ReadModel.AncillaryPriceComponents AS component WHERE component.AncillaryPricingRateId = rate.Id))
                FROM ReadModel.AncillaryPricingRates AS rate WHERE rate.AncillaryPricingId = {defined.Id}
                ORDER BY 1
                """));
        Assert.Equal(
            new[] { "Ancillary.AncillaryPriceComponents.Amount 19 6", "Ancillary.AncillaryPricingRates.BaseAmount 19 6", "ReadModel.AncillaryPriceComponents.Amount 19 6", "ReadModel.AncillaryPricingRates.BaseAmount 19 6" },
            await RowsAsync($"""
                SELECT CONCAT(s.name, '.', t.name, '.', c.name, ' ', c.precision, ' ', c.scale) AS Value
                FROM sys.columns AS c JOIN sys.tables AS t ON t.object_id = c.object_id JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                WHERE s.name IN ('Ancillary', 'ReadModel') AND t.name IN ('AncillaryPricingRates', 'AncillaryPriceComponents') AND c.name IN ('BaseAmount', 'Amount')
                ORDER BY 1
                """));

        var other = await DraftProvisionAsync(PricingUnit.PerItem);

        await RefusedAsync(16512, 422, scope => scope.DefinePricing.DefineAsync(new TestDefinePricingRatesCommand(other, [Rate(1201.5m, Jpy)])));
        await RefusedAsync(16512, 422, scope => scope.DefinePricing.DefineAsync(new TestDefinePricingRatesCommand(other, [Rate(12.345m, Eur)])));
        await RefusedAsync(16512, 422, scope => scope.DefinePricing.DefineAsync(new TestDefinePricingRatesCommand(other, [Rate(12.1251m, Kwd)])));
        await RefusedAsync(16511, 422, scope => scope.DefinePricing.DefineAsync(new TestDefinePricingRatesCommand(other, [Rate(10m, 999)])));
        Assert.Equal("0", (await RowsAsync($"SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM Ancillary.AncillaryPricings WHERE AncillaryProvisionId = {other}")).Single());
    }

    [Fact]
    public async Task PR08_PR09_PR13_a_foreign_currency_component_a_duplicate_rate_key_and_a_passenger_selector_on_a_non_passenger_unit_are_refused_and_the_key_is_unique_in_sql()
    {
        var perPassenger = await DraftProvisionAsync(PricingUnit.PerPassenger);
        var perItem = await DraftProvisionAsync(PricingUnit.PerItem);

        await RefusedAsync(16513, 422, scope => scope.DefinePricing.DefineAsync(
            new TestDefinePricingRatesCommand(perPassenger, [Rate(40m, Eur, null, Component(AncillaryPriceLineCategory.Tax, "VAT", 4m, Usd))])));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(
            new TestDefinePricingRatesCommand(perPassenger, [Rate(40m, Eur, PassengerTypeCode.ADT), Rate(41m, Eur, PassengerTypeCode.ADT)])));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(new TestDefinePricingRatesCommand(perItem, [Rate(40m, Eur, PassengerTypeCode.CHD)])));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(new TestDefinePricingRatesCommand(perItem, [Rate(40m, Eur), Rate(41m, Eur)])));

        var defined = await DefineAsync(perItem, Rate(40m, Eur), Rate(44m, Usd));

        Assert.Equal(
            new[] { "Ancillary.AncillaryPricingRates.IX_AncillaryPricingRates_RateKey 1 " },
            await RowsAsync($"""
                SELECT CONCAT(s.name, '.', t.name, '.', i.name, ' ', i.is_unique, ' ', i.filter_definition) AS Value
                FROM sys.indexes AS i JOIN sys.tables AS t ON t.object_id = i.object_id JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                WHERE i.name = 'IX_AncillaryPricingRates_RateKey' ORDER BY 1
                """));

        var duplicate = await Assert.ThrowsAsync<SqlException>(() => RequestAsync(scope => scope.Command.Database.ExecuteSqlAsync($"""
            INSERT INTO Ancillary.AncillaryPricingRates (Id, AncillaryPricingId, PassengerTypeCode, AgeFromInclusive, AgeToExclusive, CurrencyId, BaseAmount, LastUpdateTime, LastUpdatedBy)
            SELECT Id + 1000000, AncillaryPricingId, PassengerTypeCode, AgeFromInclusive, AgeToExclusive, CurrencyId, 99, LastUpdateTime, LastUpdatedBy
            FROM Ancillary.AncillaryPricingRates WHERE AncillaryPricingId = {defined.Id} AND CurrencyId = {Eur}
            """)));

        Assert.Equal(2601, duplicate.Number);
    }

    [Fact]
    public async Task PR15_PR22_the_read_model_returns_base_components_unit_total_and_the_fees_it_did_not_apply()
    {
        var provisionId = await DraftProvisionAsync(PricingUnit.PerItem);
        var defined = await DefineAsync(
            provisionId,
            Rate(
                40m,
                Eur,
                null,
                Component(AncillaryPriceLineCategory.Tax, "VAT", 4m, Eur),
                Component(AncillaryPriceLineCategory.Fee, "SVC", 1m, Eur, FeeApplicationUnit.Item),
                Component(AncillaryPriceLineCategory.Fee, "TKT", 7m, Eur, FeeApplicationUnit.Ticket)));

        await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, defined.Id)));

        var rate = (await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(defined.Id))).Rates.Single();

        Assert.Equal((40m, "EUR", 45m, "EUR", false), (rate.BasePrice.Amount, rate.BasePrice.Currency, rate.UnitTotal.Amount, rate.UnitTotal.Currency, rate.IsUnitTotalComplete));
        Assert.Equal(
            new[] { "Fee SVC 1 EUR Item", "Fee TKT 7 EUR Ticket", "Tax VAT 4 EUR " },
            rate.Components
                .Select(component => FormattableString.Invariant($"{component.Category.Name} {component.Code} {component.Amount.Amount:0.##} {component.Amount.Currency} {component.FeeApplicationUnit?.Name}"))
                .Order(StringComparer.Ordinal));
        Assert.Equal(("TKT", 7m, "Ticket"), rate.UnappliedFees.Select(fee => (fee.Code!, fee.Amount.Amount, fee.FeeApplicationUnit!.Name)).Single());
        Assert.DoesNotContain(typeof(BackofficePricingRateDto).GetProperties(), property => property.Name is "TotalAmount" or "GrandTotal");
    }

    [Fact]
    public async Task PR18_PR19_a_paid_provision_is_not_published_without_an_active_price_and_a_free_one_never_takes_a_price()
    {
        var paid = await DraftProvisionAsync(PricingUnit.PerItem);
        var free = await DraftProvisionAsync(PricingUnit.PerItem, CommercialDisposition.Free);
        var notAvailable = await DraftProvisionAsync(PricingUnit.PerItem, CommercialDisposition.NotAvailable);

        await RefusedAsync(16310, 409, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(paid)));

        var draft = await DefineAsync(paid, Rate(40m, Eur));

        await RefusedAsync(16310, 409, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(paid)));
        await RefusedAsync(16505, 409, scope => scope.DefinePricing.DefineAsync(new TestDefinePricingRatesCommand(free, [Rate(40m, Eur)])));
        await RefusedAsync(16505, 409, scope => scope.DefinePricing.DefineAsync(new TestDefinePricingRatesCommand(notAvailable, [Rate(40m, Eur)])));

        await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(paid, draft.Id)));
        await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(free)));
        await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(notAvailable)));

        Assert.Equal(
            new[] { $"{paid} 2 1", $"{free} 2 0", $"{notAvailable} 2 0" },
            await RowsAsync($"""
                SELECT CONCAT(provision.Id, ' ', provision.Status, ' ', (SELECT COUNT(*) FROM Ancillary.AncillaryPricings AS pricing WHERE pricing.AncillaryProvisionId = provision.Id AND pricing.Status = 2)) AS Value
                FROM Ancillary.AncillaryProvisions AS provision WHERE provision.Id IN ({paid}, {free}, {notAvailable}) ORDER BY provision.Id
                """));
    }

    [Fact]
    public async Task PR21_two_concurrent_price_activations_have_one_winner_behind_the_filtered_unique_index()
    {
        var provisionId = await DraftProvisionAsync(PricingUnit.PerItem);
        var first = await DefineAsync(provisionId, Rate(40m, Eur));
        var second = await DefineAsync(provisionId, Rate(42m, Eur), Rate(46m, Usd));

        await using (var loser = new AncillaryScope(_database, _clock))
        {
            var losing = (await loser.Pricings.GetAsync(first.Id))!;

            losing.Activate(Scales, _clock.Now);

            await RequestAsync(scope => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(second.Id)));
            await BusinessAssert.ThrowsAsync(16507, 409, () => loser.UnitOfWork.SaveChangesAsync());
        }

        Assert.Equal(
            new[] { $"{first.Id} 1", $"{second.Id} 2" },
            await RowsAsync($"SELECT CONCAT(Id, ' ', Status) AS Value FROM Ancillary.AncillaryPricings WHERE AncillaryProvisionId = {provisionId} ORDER BY Version"));
        Assert.Equal(
            new[] { "Ancillary IX_AncillaryPricings_OneActivePerProvision 1 ([Status]=(2))" },
            await RowsAsync($"""
                SELECT CONCAT(s.name, ' ', i.name, ' ', i.is_unique, ' ', i.filter_definition) AS Value
                FROM sys.indexes AS i JOIN sys.tables AS t ON t.object_id = i.object_id JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                WHERE i.name = 'IX_AncillaryPricings_OneActivePerProvision' ORDER BY 1
                """));
    }

    [Fact]
    public void PR24_the_backoffice_contract_has_no_flat_price_line_or_root_currency_and_a_payload_without_rates_is_refused()
    {
        var validator = new BackofficeDefineAncillaryPricingCommandValidator();

        Assert.Equal(
            new[] { nameof(BackofficeDefineAncillaryPricingCommand.AncillaryProvisionId), nameof(BackofficeDefineAncillaryPricingCommand.Rates) },
            typeof(BackofficeDefineAncillaryPricingCommand).GetProperties().Select(property => property.Name).Order(StringComparer.Ordinal));
        Assert.Equal(
            new[] { "AgeFromInclusive", "AgeToExclusive", "BasePrice", "Components", "PassengerTypeCode" },
            typeof(PricingRateInput).GetProperties().Select(property => property.Name).Order(StringComparer.Ordinal));
        Assert.Contains("Rates", validator.Validate(new BackofficeDefineAncillaryPricingCommand(1, null!)).Errors.Select(error => error.PropertyName));
        Assert.Contains(
            "Rates[0].BasePrice",
            validator.Validate(new BackofficeDefineAncillaryPricingCommand(1, [new PricingRateInput(null, null, null, null!, null)])).Errors.Select(error => error.PropertyName));
    }

    [Fact]
    public async Task PR24_PR25_a_flat_base_component_and_a_percentage_fee_unit_are_refused_by_code_and_never_reinterpreted()
    {
        var provisionId = await DraftProvisionAsync(PricingUnit.PerItem);

        await RefusedAsync(16514, 422, scope => scope.DefinePricing.DefineAsync(
            new TestDefinePricingRatesCommand(provisionId, [Rate(40m, Eur, null, Component(AncillaryPriceLineCategory.Ancillary, "BASE", 40m, Eur))])));

        var percent = await DefineAsync(provisionId, Rate(40m, Eur, null, Component(AncillaryPriceLineCategory.Fee, "PCT", 1m, Eur, FeeApplicationUnit.OnePercentOfFarePerKilogram)));

        await RefusedAsync(16305, 422, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, percent.Id)));
        await RefusedAsync(16305, 422, scope => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(percent.Id)));
        Assert.Equal(
            new[] { $"{percent.Id} 1" },
            await RowsAsync($"SELECT CONCAT(Id, ' ', Status) AS Value FROM Ancillary.AncillaryPricings WHERE AncillaryProvisionId = {provisionId}"));
    }
}
