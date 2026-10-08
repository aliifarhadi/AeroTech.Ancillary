using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V121Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Families;

[Collection(DatabaseCollection.Name)]
public class V121PriceStressAcceptanceTests
{
    private const long AirlineOffice = 9200000000000001;
    private const long AgencyOffice = 1551571720488353792;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V121PriceStressAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private Task RefusedAsync(int code, int httpStatus, Func<AncillaryScope, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, async () =>
        {
            await using var scope = new AncillaryScope(_database, _clock);

            await request(scope);
        });

    private async Task<long> DefinitionAsync(string reference, PricingUnit pricingUnit = PricingUnit.PerPassenger, ServiceDateBasis basis = ServiceDateBasis.FlightDeparture)
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        return (await _proof.DefinitionAsync(CarrierDefinition(airlineId, supplierId, reference, "SVC", "F", "TS", reference, pricingUnit: pricingUnit, serviceDateBasis: basis))).Id;
    }

    private async Task<(int Sequence, string Rules, string Filed)[]> FiledAsync(long serviceDefinitionId)
    {
        var filed = new List<(int, string, string)>();

        await using var reader = new AncillaryScope(_database, _clock);

        foreach (var row in (await _proof.ListedProvisionsAsync(serviceDefinitionId)).Where(row => row.Status.Name == "Active").OrderBy(row => row.Sequence))
        {
            var provisionId = long.Parse(row.Id);
            var active = await reader.Pricings.FindActiveAsync(provisionId);

            filed.Add((
                row.Sequence,
                RuleText.Of(await reader.GetProvisionById.ExecuteAsync(provisionId)),
                active is null ? row.Disposition.Name : RuleText.Of(await reader.GetPricingById.ExecuteAsync(active.Id))));
        }

        return filed.ToArray();
    }

    [Fact]
    public async Task V121_P17_two_seasonal_prices_are_two_dated_provisions_each_with_one_active_price_and_never_two_prices_on_one_provision()
    {
        var definitionId = await DefinitionAsync("LOUNGE_SEASONAL", basis: ServiceDateBasis.ServiceStart);
        var summer = await _proof.RuleAsync(
            Provision(definitionId, 10) with { TravelDate = Dates([Period(Day(2027, 6, 1), Day(2027, 8, 31))]) },
            provisionId => Pricing(provisionId, Eur, Base(30m)));
        var winter = await _proof.RuleAsync(
            Provision(definitionId, 20) with { TravelDate = Dates([Period(Day(2027, 12, 1), Day(2028, 2, 29))]) },
            provisionId => Pricing(provisionId, Eur, Base(20m)));
        var standard = await _proof.RuleAsync(Provision(definitionId, 100), provisionId => Pricing(provisionId, Eur, Base(25m)));

        Assert.Equal(
            new[]
            {
                (10, "PERMIT=2027-06-01..2027-08-31", "PerPassenger EUR *[-]=30+0+0=30"),
                (20, "PERMIT=2027-12-01..2028-02-29", "PerPassenger EUR *[-]=20+0+0=20"),
                (100, "", "PerPassenger EUR *[-]=25+0+0=25")
            },
            await FiledAsync(definitionId));

        long secondPrice;

        await using (var author = new AncillaryScope(_database, _clock))
            secondPrice = (await author.DefinePricing.DefineAsync(Pricing(summer.Provision.Id, Eur, Base(35m)))).Id;

        await RefusedAsync(16507, 409, scope => scope.ActivatePricing.ActivateAsync(new TestPricingLifecycleCommand(secondPrice)));

        await using (var switcher = new AncillaryScope(_database, _clock))
            await switcher.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(summer.Provision.Id, secondPrice, summer.Pricing!.Id));

        Assert.Equal("PerPassenger EUR *[-]=35+0+0=35", (await FiledAsync(definitionId))[0].Filed);
        Assert.Equal("PerPassenger EUR *[-]=20+0+0=20", (await FiledAsync(definitionId))[1].Filed);
        Assert.NotEqual(winter.Pricing!.Id, standard.Pricing!.Id);
    }

    [Fact]
    public async Task V121_REQ_travel_dates_flights_routes_fare_families_cabins_and_rbds_carry_their_own_filed_price()
    {
        var definitionId = await DefinitionAsync("XBAG_RULES", PricingUnit.PerPiece);

        TestDefineProvisionCommand Bag(int sequence, CommercialDisposition disposition = CommercialDisposition.Paid)
            => Provision(definitionId, sequence, disposition, quantityUnit: AncillaryQuantityUnit.Piece, applicationType: ProvisionApplicationType.Baggage) with { BaggageApplication = Baggage(23m, 1, 1) };

        await _proof.RuleAsync(
            Bag(10) with { FlightApplication = new(AllowedFlightIds: [81234]), TravelDate = Dates([Period(Day(2027, 3, 20), Day(2027, 4, 2))]) },
            provisionId => Pricing(provisionId, Eur, Base(45m)));
        await _proof.RuleAsync(
            Bag(20) with { Geography = new(AllowedRoutePairs: [Pair(Thr, Ist)]), FareApplication = new(AllowedFareFamilyIds: [5]) },
            provisionId => Pricing(provisionId, Eur, Base(35m)));
        await _proof.RuleAsync(
            Bag(30) with { FareApplication = new(AllowedCabinClassIds: [3], AllowedRbdIds: [41, 42]) },
            provisionId => Pricing(provisionId, Eur, Base(20m)));
        await _proof.RuleAsync(Bag(40, CommercialDisposition.Free) with { FareApplication = new(AllowedFareFamilyIds: [9]) });
        await _proof.RuleAsync(Bag(100), provisionId => Pricing(provisionId, Eur, Base(30m), Tax("VAT", 3m)));

        Assert.Equal(
            new[]
            {
                (10, "FLT=81234; PERMIT=2027-03-20..2027-04-02; BAG=1-1:23Kg:Prepaid", "PerPiece EUR *[-]=45+0+0=45"),
                (20, "ROUTE=1>6; FAMILY=5; BAG=1-1:23Kg:Prepaid", "PerPiece EUR *[-]=35+0+0=35"),
                (30, "CABIN=3; RBD=41,42; BAG=1-1:23Kg:Prepaid", "PerPiece EUR *[-]=20+0+0=20"),
                (40, "FAMILY=9; BAG=1-1:23Kg:Prepaid", "Free"),
                (100, "BAG=1-1:23Kg:Prepaid", "PerPiece EUR *[-]=30+3+0=33")
            },
            await FiledAsync(definitionId));
    }

    [Fact]
    public async Task V121_REQ_points_of_sale_customers_customer_types_and_passengers_carry_their_own_filed_price_and_currency()
    {
        var definitionId = await DefinitionAsync("LOUNGE_BY_SELLER", basis: ServiceDateBasis.ServiceStart);

        await _proof.RuleAsync(
            Provision(definitionId, 5, CommercialDisposition.NotAvailable) with { SalesRestrictions = new(AllowedCustomerIds: [9001]) });
        await _proof.RuleAsync(
            Provision(definitionId, 10) with { SalesRestrictions = new(AllowedPointOfSaleIds: [AgencyOffice], AllowedCustomerTypes: [CustomerType.TravelAgency]) },
            provisionId => Pricing(provisionId, Usd, Base(22m, PassengerTypeCode.ADT), Base(12m, PassengerTypeCode.CHD)));
        await _proof.RuleAsync(
            Provision(definitionId, 20) with { SalesRestrictions = new(AllowedPointOfSaleIds: [AirlineOffice]) },
            provisionId => Pricing(provisionId, Irr, Base(2500000m, PassengerTypeCode.ADT), Base(1500000m, PassengerTypeCode.CHD)));
        await _proof.RuleAsync(
            Provision(definitionId, 30, CommercialDisposition.Free) with { PassengerEligibility = Passengers(PassengerTypeCode.INF) });
        await _proof.RuleAsync(
            Provision(definitionId, 100),
            provisionId => Pricing(provisionId, Eur, Base(25m, PassengerTypeCode.ADT), Base(15m, PassengerTypeCode.CHD)));

        Assert.Equal(
            new[]
            {
                (5, "CUST=9001", "NotAvailable"),
                (10, $"POS={AgencyOffice}; CUSTTYPE=TravelAgency", "PerPassenger USD ADT[-]=22+0+0=22 | CHD[-]=12+0+0=12"),
                (20, $"POS={AirlineOffice}", "PerPassenger IRR ADT[-]=2500000+0+0=2500000 | CHD[-]=1500000+0+0=1500000"),
                (30, "PTC=INF", "Free"),
                (100, "", "PerPassenger EUR ADT[-]=25+0+0=25 | CHD[-]=15+0+0=15")
            },
            await FiledAsync(definitionId));
    }
}
