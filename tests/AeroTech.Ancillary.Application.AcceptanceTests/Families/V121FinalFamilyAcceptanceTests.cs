using System.Collections.Concurrent;
using System.Globalization;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P2Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V121Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Families;

public sealed record FinalFamilyOutcome(string Definition, string Rule, string Price, string Policy);

public sealed class FinalFamilyCache
{
    private readonly ConcurrentDictionary<string, Lazy<Task<FinalFamilyOutcome>>> _outcomes = new();

    public Task<FinalFamilyOutcome> GetAsync(string family, Func<Task<FinalFamilyOutcome>> build)
        => _outcomes.GetOrAdd(family, _ => new Lazy<Task<FinalFamilyOutcome>>(build)).Value;
}

[Collection(DatabaseCollection.Name)]
public class V121FinalFamilyAcceptanceTests : IClassFixture<FinalFamilyCache>
{
    private const long Flight = 81234;
    private const long OtherFlight = 81240;
    private const long GermanOffice = 9200000000000049;
    private const long TurkishOffice = 9200000000000090;
    private const int Gbp = 53;
    private const int Kwd = 82;
    private const int Turkey = 90;
    private const string NoSources = "0|0|0|0|0|0";
    private const string UnlimitedPolicy = "Unlimited offline=Active state=Unlimited/ check=False guaranteed=False configured=/ sources=" + NoSources;
    private const string CheckedUnlimitedPolicy = "Unlimited offline=Active state=Unlimited/ check=True guaranteed=False configured=/ sources=" + NoSources;
    private const string SupplierPolicy = "Supplier offline=Active state=DelegatedCheckRequired/DelegatedSourceNotConnected check=False guaranteed=False configured=/ sources=" + NoSources;

    private static readonly DateTimeOffset At = Utc(10, 15);

    private sealed record Family(
        string Reference,
        string SubCode,
        string TypeCode,
        string GroupCode,
        PricingUnit PricingUnit,
        ServiceDateBasis Basis,
        ServiceDefinitionBookingInput? Booking,
        Func<long, TestDefineProvisionCommand> Rule,
        IReadOnlyList<PricingRateInput>? Rates,
        InventoryAuthority Authority,
        string Definition,
        string Rules,
        string Price,
        string Policy,
        string? ProviderKey = null,
        LocalInventoryPattern? Pattern = null,
        FlightCountConsumptionInput? Count = null,
        AirportSlotConsumptionInput? Slot = null,
        Action<InventoryFixture>? Connect = null);

    private static readonly IReadOnlyDictionary<string, string> Variants = new Dictionary<string, string>
    {
        ["F01"] = "A02", ["F02"] = "A01", ["F03"] = "A03", ["F04"] = "A06", ["F05"] = "A13", ["F06"] = "A08", ["F07"] = "A11",
        ["F08"] = "A15", ["F09"] = "A19", ["F10"] = "A23", ["F11"] = "A20", ["F12"] = "A22", ["F13"] = "A24"
    };

    private static readonly IReadOnlyDictionary<string, Family> Catalog = new Dictionary<string, Family>
    {
        ["F01"] = new(
            "XBAG_WEIGHT_10KG", "XW1", "C", "BG", PricingUnit.PerItem, ServiceDateBasis.FlightDeparture, null,
            id => Provision(id, 10, coverageScope: ServiceCoverageScope.Journey, maxQuantity: 2, applicationType: ProvisionApplicationType.Baggage) with
            {
                Geography = new(AllowedRoutePairs: [Pair(Thr, Ist)]),
                AdvancePurchase = new(6, TimeUnit.Hours, MaximumPeriod: 720),
                BaggageApplication = Baggage(10m, chargeKind: BaggageChargeKind.WeightPackage, allowanceConcept: BaggageAllowanceConcept.Weight)
            },
            [Rate(25m, Eur), Rate(27.5m, Usd)],
            InventoryAuthority.Unlimited,
            "PerItem FlightDeparture NoBookingProcessRequired/- Immediate None wrongUnit=16312",
            "Both Paid Journey 1-2Each book=False check=False | ROUTE=1>6; ADVANCE=6Hours; BAG=-:10Kg:Prepaid | MAXADV=720 | KIND=WeightPackage/Weight",
            "*[-] 25 EUR = 25 EUR | *[-] 27.5 USD = 27.5 USD",
            UnlimitedPolicy),
        ["F02"] = new(
            "XBAG_PIECE_23KG", "XP2", "C", "BG", PricingUnit.PerPiece, ServiceDateBasis.FlightDeparture, Ssr("XBAG"),
            id => Provision(id, 10, coverageScope: ServiceCoverageScope.Journey, quantityUnit: AncillaryQuantityUnit.Piece, maxQuantity: 3, applicationType: ProvisionApplicationType.Baggage) with
            {
                FlightApplication = new(AllowedFlightIds: [Flight]),
                FareApplication = new(AllowedFareFamilyIds: [5]),
                BaggageApplication = Baggage(23m, 1, 3)
            },
            [Rate(30m, Eur, components: [Tax("VAT", 2.7m, Eur), Fee("HDL", 1m, Eur, FeeApplicationUnit.Item)])],
            InventoryAuthority.Unlimited,
            "PerPiece FlightDeparture Ssr/XBAG Immediate None wrongUnit=16312",
            "Both Paid Journey 1-3Piece book=False check=False | FLT=81234; FAMILY=5; BAG=1-3:23Kg:Prepaid | KIND=ExtraPiece/Piece",
            "*[-] 30 EUR +Fee:HDL:1/Item +Tax:VAT:2.7 = 33.7 EUR",
            UnlimitedPolicy),
        ["F03"] = new(
            "XBAG_OVERWEIGHT", "XOW", "C", "BG", PricingUnit.PerPiece, ServiceDateBasis.FlightDeparture, null,
            id => Provision(id, 10, quantityUnit: AncillaryQuantityUnit.Piece, applicationType: ProvisionApplicationType.Baggage) with
            {
                FlightApplication = new(AllowedAircraftIds: [3]),
                BaggageApplication = Baggage(32m, chargeKind: BaggageChargeKind.Overweight, allowanceConcept: null)
            },
            [Rate(60m, Usd)],
            InventoryAuthority.Unlimited,
            "PerPiece FlightDeparture NoBookingProcessRequired/- Immediate None wrongUnit=16312",
            "Both Paid Sector 1-1Piece book=False check=False | ACFT=3; BAG=-:32Kg:Prepaid | KIND=Overweight/-",
            "*[-] 60 USD = 60 USD",
            UnlimitedPolicy),
        ["F04"] = new(
            "SPORT_BICYCLE", "BIK", "C", "SP", PricingUnit.PerPiece, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "BIKE", null, ConfirmationRequirement.SubjectToConfirmation),
            id => Provision(id, 10, quantityUnit: AncillaryQuantityUnit.Piece, applicationType: ProvisionApplicationType.Baggage, bookingRequired: true) with
            {
                PurchaseStage = PurchaseStage.PreOrder,
                Availability = new(true),
                AdvancePurchase = new(48, TimeUnit.Hours),
                BaggageApplication = Baggage(null, 1, 1, chargeKind: BaggageChargeKind.SpecialEquipment, allowanceConcept: null)
            },
            [Rate(50m, Eur)],
            InventoryAuthority.Supplier,
            "PerPiece FlightDeparture Ssr/BIKE SubjectToConfirmation None wrongUnit=16312",
            "PreOrder Paid Sector 1-1Piece book=True check=True | ADVANCE=48Hours; BAG=1-1:Kg:Prepaid | KIND=SpecialEquipment/-",
            "*[-] 50 EUR = 50 EUR",
            "Supplier offline=Active state=DelegatedCheckRequired/DelegatedSourceNotConnected check=True guaranteed=False configured=/ sources=" + NoSources,
            "GroundHandlerA"),
        ["F05"] = new(
            "PET_IN_CABIN", "PET", "C", "PT", PricingUnit.PerItem, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "PETC", null, ConfirmationRequirement.SubjectToConfirmation),
            id => Provision(id, 10, coverageScope: ServiceCoverageScope.Journey, bookingRequired: true) with
            {
                Availability = new(true),
                Geography = new(AllowedRoutePairs: [Pair(Thr, Ist, RoutePairDirection.BothDirections)])
            },
            [Rate(55m, Eur)],
            InventoryAuthority.Local,
            "PerItem FlightDeparture Ssr/PETC SubjectToConfirmation None wrongUnit=16312",
            "Both Paid Journey 1-1Each book=True check=True | ROUTE=1<>6",
            "*[-] 55 EUR = 55 EUR",
            "Local/FlightCount offline=16608>NotConfigured state=NotConfigured/SourceNotConfigured check=True guaranteed=False configured=/ sources=" + NoSources,
            Pattern: LocalInventoryPattern.FlightCount,
            Count: Count(),
            Connect: fixture => fixture.Resources = [(InventoryResourceKind.FlightCount, PetResource)]),
        ["F06"] = new(
            "SEAT_EXTRA_LEGROOM", "SXL", "F", "SA", PricingUnit.PerSeat, ServiceDateBasis.FlightDeparture, null,
            id => Provision(id, 10, applicationType: ProvisionApplicationType.Seat) with
            {
                FlightApplication = new(AllowedAircraftIds: [3]),
                SeatApplication = Seat(["18A"], ["L"])
            },
            [Rate(18m, Eur)],
            InventoryAuthority.FlightFlow,
            "PerSeat FlightDeparture NoBookingProcessRequired/- Immediate None wrongUnit=16312",
            "Both Paid Sector 1-1Each book=False check=False | ACFT=3; SEAT=18A; SEATCHAR=L",
            "*[-] 18 EUR = 18 EUR",
            "FlightFlow offline=16608>NotConfigured state=DelegatedCheckRequired/DelegatedSourceNotConnected check=False guaranteed=False configured=/ sources=" + NoSources,
            "FlightFlow",
            Connect: fixture => fixture.FlightFlowProviderKeys = ["FlightFlow"]),
        ["F07"] = new(
            "MEAL_VGML", "MVG", "F", "ML", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, Ssr("VGML"),
            id => Provision(id, 10, CommercialDisposition.Free, bookingRequired: true) with
            {
                PassengerEligibility = Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
                AdvancePurchase = new(24, TimeUnit.Hours)
            },
            null,
            InventoryAuthority.Unlimited,
            "PerPassenger FlightDeparture Ssr/VGML Immediate None wrongUnit=16312",
            "Both Free Sector 1-1Each book=True check=False | PTC=ADT,CHD; ADVANCE=24Hours",
            "- price=16505 active=0",
            UnlimitedPolicy),
        ["F08"] = new(
            "ASSIST_WCHR", "WCR", "F", "MA", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "WCHR", null, ConfirmationRequirement.SubjectToConfirmation),
            id => Provision(id, 10, CommercialDisposition.Free, bookingRequired: true) with { Availability = new(true) },
            null,
            InventoryAuthority.Unlimited,
            "PerPassenger FlightDeparture Ssr/WCHR SubjectToConfirmation None wrongUnit=16312",
            "Both Free Sector 1-1Each book=True check=True",
            "- price=16505 active=0",
            CheckedUnlimitedPolicy),
        ["F09"] = new(
            "UMNR_SERVICE", "UMN", "F", "UN", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            new ServiceDefinitionBookingInput(BookingMethod.Ssr, "UMNR", null, ConfirmationRequirement.SubjectToConfirmation),
            id => Provision(id, 10, bookingRequired: true) with
            {
                PurchaseStage = PurchaseStage.PreOrder,
                Availability = new(true),
                PassengerEligibility = new(AllowedAgeBands: [new(5, 12)])
            },
            [Rate(60m, Eur, ageFrom: 5, ageTo: 8), Rate(50m, Eur, ageFrom: 8, ageTo: 12)],
            InventoryAuthority.Unlimited,
            "PerPassenger FlightDeparture Ssr/UMNR SubjectToConfirmation None wrongUnit=16312",
            "PreOrder Paid Sector 1-1Each book=True check=True | AGE=5-12",
            "*[5-8] 60 EUR = 60 EUR | *[8-12] 50 EUR = 50 EUR",
            CheckedUnlimitedPolicy),
        ["F10"] = new(
            "PRIORITY_BOARDING", "PRB", "F", "TS", PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, null,
            id => Provision(id, 10) with
            {
                PurchaseStage = PurchaseStage.PostTicketed,
                FareApplication = new(AllowedFareFamilyIds: [5, 6]),
                TravelDate = Dates(blackout: [Period(Day(2027, 3, 20), Day(2027, 4, 5))])
            },
            [Rate(8m, Eur), Rate(7m, Gbp)],
            InventoryAuthority.Unlimited,
            "PerPassenger FlightDeparture NoBookingProcessRequired/- Immediate None wrongUnit=16312",
            "PostTicketed Paid Sector 1-1Each book=False check=False | FAMILY=5,6; BLACKOUT=2027-03-20..2027-04-05",
            "*[-] 8 EUR = 8 EUR | *[-] 7 GBP = 7 GBP",
            UnlimitedPolicy),
        ["F11"] = new(
            "LOUNGE_IKA", "LNG", "F", "LG", PricingUnit.PerPassenger, ServiceDateBasis.ServiceStart, null,
            id => Provision(id, 10) with
            {
                PassengerEligibility = Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
                Geography = new(ServiceLocations: [Location(ServiceLocationType.Airport, Ika)]),
                DayTimeApplication = DayTime(Window(EveryDay, 6, 23))
            },
            [Rate(30m, Eur, PassengerTypeCode.ADT), Rate(15m, Eur, PassengerTypeCode.CHD)],
            InventoryAuthority.Supplier,
            "PerPassenger ServiceStart NoBookingProcessRequired/- Immediate None wrongUnit=16312",
            "Both Paid Sector 1-1Each book=False check=False | PTC=ADT,CHD; AT=Airport:2; TIME=127:6-23:Allow",
            "ADT[-] 30 EUR = 30 EUR | CHD[-] 15 EUR = 15 EUR",
            SupplierPolicy,
            "LoungePartnerA"),
        ["F12"] = new(
            "CIP_IKA", "CIP", "F", "TS", PricingUnit.PerPassenger, ServiceDateBasis.ServiceStart, null,
            id => Provision(id, 10) with
            {
                Geography = new(ServiceLocations: [Location(ServiceLocationType.Airport, Ika)]),
                AdvancePurchase = new(12, TimeUnit.Hours)
            },
            [Rate(80m, Eur), Rate(88m, Usd)],
            InventoryAuthority.Local,
            "PerPassenger ServiceStart NoBookingProcessRequired/- Immediate None wrongUnit=16312",
            "Both Paid Sector 1-1Each book=False check=False | AT=Airport:2; ADVANCE=12Hours",
            "*[-] 80 EUR = 80 EUR | *[-] 88 USD = 88 USD",
            "Local/AirportSlot offline=16608>NotConfigured state=NotConfigured/SourceNotConfigured check=False guaranteed=False configured=/ sources=" + NoSources,
            Pattern: LocalInventoryPattern.AirportSlot,
            Slot: SlotUse(),
            Connect: fixture => fixture.Facilities = new() { [Lounge] = (Ika, "Asia/Tehran") }),
        ["F13"] = new(
            "WIFI_FULL", "WIF", "F", "IE", PricingUnit.PerItem, ServiceDateBasis.FlightDeparture, null,
            id => Provision(id, 10, maxQuantity: 4) with { FlightApplication = new(AllowedFlightIds: [Flight], AllowedAircraftIds: [3]) },
            [Rate(9.99m, Usd)],
            InventoryAuthority.Unlimited,
            "PerItem FlightDeparture NoBookingProcessRequired/- Immediate None wrongUnit=16312",
            "Both Paid Sector 1-4Each book=False check=False | FLT=81234; ACFT=3",
            "*[-] 9.99 USD = 9.99 USD",
            UnlimitedPolicy)
    };

    private readonly TestDatabase _database;
    private readonly FinalFamilyCache _cache;

    public V121FinalFamilyAcceptanceTests(TestDatabase database, FinalFamilyCache cache)
    {
        _database = database;
        _cache = cache;
    }

    public static IEnumerable<object[]> Families => Catalog.Keys.Select(family => new object[] { family });

    private static PricingRateInput Rate(
        decimal amount,
        int currencyId,
        PassengerTypeCode? passengerTypeCode = null,
        int? ageFrom = null,
        int? ageTo = null,
        IReadOnlyList<PriceComponentInput>? components = null)
        => new(passengerTypeCode, ageFrom, ageTo, new MoneyInput(amount, currencyId), components);

    private static PriceComponentInput Tax(string code, decimal amount, int currencyId)
        => new(AncillaryPriceLineCategory.Tax, code, null, null, null, new MoneyInput(amount, currencyId), null, null, TaxTreatment.AddedToBase);

    private static PriceComponentInput Fee(string code, decimal amount, int currencyId, FeeApplicationUnit unit)
        => new(AncillaryPriceLineCategory.Fee, code, null, null, null, new MoneyInput(amount, currencyId), unit, null);

    private static string Money(MoneyDto money) => string.Create(CultureInfo.InvariantCulture, $"{money.Amount:0.###} {money.Currency}");

    private static string DefinitionText(BackofficeServiceDefinitionDto definition, string wrongUnit)
        => $"{definition.PricingUnit!.Name} {definition.ServiceDateBasis!.Name} {definition.BookingMethod.Name}/{definition.BookingSsrCode ?? "-"} " +
           $"{definition.BookingConfirmationRequirement.Name} {definition.DocumentType.Name} wrongUnit={wrongUnit}";

    private static string RuleLine(BackofficeProvisionDto provision)
    {
        var parts = new List<string>
        {
            $"{provision.PurchaseStage.Name} {provision.Disposition.Name} {provision.CoverageScope.Name} {provision.MinQuantity}-{provision.MaxQuantity}{provision.QuantityUnit.Name} " +
            $"book={provision.BookingRequired} check={provision.MustCheckAvailability}"
        };

        if (RuleText.Of(provision) is { Length: > 0 } rules)
            parts.Add(rules);

        if (provision.AdvancePurchase?.MaximumPeriod is { } maximum)
            parts.Add($"MAXADV={maximum}");

        if (provision.BaggageApplication is { } baggage)
            parts.Add($"KIND={baggage.ChargeKind?.Name ?? "-"}/{baggage.AllowanceConcept?.Name ?? "-"}");

        return string.Join(" | ", parts);
    }

    private static string PriceText(BackofficePricingDto pricing)
        => string.Join(
            " | ",
            pricing.Rates
                .OrderBy(rate => rate.BasePrice.CurrencyId)
                .ThenBy(rate => rate.PassengerTypeCode?.Name, StringComparer.Ordinal)
                .ThenBy(rate => rate.AgeFromInclusive)
                .Select(rate =>
                {
                    var components = string.Concat(rate.Components
                        .OrderBy(component => component.Category.Name, StringComparer.Ordinal)
                        .ThenBy(component => component.Code, StringComparer.Ordinal)
                        .Select(component => string.Create(
                            CultureInfo.InvariantCulture,
                            $" +{component.Category.Name}:{component.Code}:{component.Amount.Amount:0.###}{(component.FeeApplicationUnit is { } unit ? "/" + unit.Name : string.Empty)}")));

                    return $"{rate.PassengerTypeCode?.Name ?? "*"}[{rate.AgeFromInclusive}-{rate.AgeToExclusive}] {Money(rate.BasePrice)}{components} = {Money(rate.UnitTotal)}";
                }));

    private static async Task<string> OutcomeAsync(Func<Task> request)
    {
        try
        {
            await request();

            return "Active";
        }
        catch (BusinessException exception)
        {
            return exception.Code.ToString(CultureInfo.InvariantCulture);
        }
    }

    private static async Task<string> SourceRowsAsync(InventoryHarness harness, int airlineId)
        => (await harness.RowsAsync($"""
            SELECT CONCAT(
                (SELECT COUNT(*) FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM Ancillary.FlightWeightInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM Ancillary.AirportSlotInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM ReadModel.FlightCountInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM ReadModel.FlightWeightInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM ReadModel.AirportSlotInventories WHERE OwnerAirlineId = {airlineId})) AS Value
            """)).Single();

    private async Task<TResult> RequestAsync<TResult>(FixedClock clock, Func<AncillaryScope, Task<TResult>> request)
    {
        await using var scope = new AncillaryScope(_database, clock);

        return await request(scope);
    }

    private Task<InventoryPolicyResult> ActivatePolicyAsync(InventoryHarness harness, InventoryFixture fixture, long policyId, long expectedVersion = 1)
        => harness.RequestAsync(fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(policyId, expectedVersion)));

    private async Task<string> PolicyTextAsync(
        InventoryHarness harness,
        InventoryFixture fixture,
        TestDefineInventoryPolicyCommand command,
        Action<InventoryFixture>? connect)
    {
        var draft = await harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(command));
        var offline = await OutcomeAsync(() => ActivatePolicyAsync(harness, fixture, draft.Id));

        if (offline != "Active")
        {
            offline += ">" + (await harness.SnapshotAsync(fixture, command.ServiceDefinitionRef, Flight, At)).State.Name;
            connect!(fixture);
            await ActivatePolicyAsync(harness, fixture, draft.Id);
        }

        var snapshot = await harness.SnapshotAsync(fixture, command.ServiceDefinitionRef, Flight, At);
        var sources = await SourceRowsAsync(harness, command.OwnerAirlineId);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{snapshot.Authority!.Name}{(snapshot.Pattern is { } pattern ? "/" + pattern.Name : string.Empty)} offline={offline} state={snapshot.State.Name}/{snapshot.ReasonCode} " +
            $"check={snapshot.RequiresAvailabilityCheck} guaranteed={snapshot.IsGuaranteed} configured={snapshot.ConfiguredCount}/{snapshot.ConfiguredKg} " +
            $"sources={sources}");
    }

    private async Task<FinalFamilyOutcome> BuildAsync(Family family, string variant)
    {
        var clock = new FixedClock();
        var harness = new InventoryHarness(_database, clock);
        var airlineId = _database.NextAirlineId();
        var fixture = new InventoryFixture(airlineId);
        var supplierId = await harness.Proof.SupplierAsync(
            family.Authority == InventoryAuthority.Supplier
                ? new TestRegisterSupplierCommand(airlineId, $"{family.Reference} partner", SupplierFulfillmentKind.External, family.ProviderKey)
                : M1Commands.LocalSupplier(airlineId));
        var definition = await harness.Proof.DefinitionAsync(CarrierDefinition(
            airlineId,
            supplierId,
            family.Reference,
            family.SubCode,
            family.TypeCode,
            family.GroupCode,
            family.Reference,
            family.Booking,
            pricingUnit: family.PricingUnit,
            serviceDateBasis: family.Basis,
            variant: variant));
        var command = family.Rule(definition.Id);
        var wrongUnit = command.Quantity.Unit == AncillaryQuantityUnit.Each ? AncillaryQuantityUnit.Kilogram : AncillaryQuantityUnit.Each;
        var probe = await RequestAsync(clock, scope => scope.DefineProvision.DefineAsync(Provision(definition.Id, 900, CommercialDisposition.Free, quantityUnit: wrongUnit)));
        var refusedUnit = await OutcomeAsync(() => RequestAsync(clock, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(probe.Id))));
        var published = await harness.Proof.RuleAsync(command, family.Rates is { } rates ? provisionId => new TestDefinePricingRatesCommand(provisionId, rates) : null);
        string price;

        if (published.Pricing is { } pricing)
        {
            price = PriceText(pricing);
        }
        else
        {
            var refusedPrice = await OutcomeAsync(() => RequestAsync(clock, scope => scope.DefinePricing.DefineAsync(
                new TestDefinePricingRatesCommand(published.Provision.Id, [Rate(1m, Eur)]))));
            var active = await RequestAsync(clock, scope => scope.Command.AncillaryPricings.AsNoTracking()
                .CountAsync(row => row.AncillaryProvisionId == published.Provision.Id && row.Status == PricingStatus.Active));

            price = $"- price={refusedPrice} active={active}";
        }

        var policy = await PolicyTextAsync(
            harness,
            fixture,
            new TestDefineInventoryPolicyCommand(
                airlineId,
                family.Reference,
                definition.Id,
                family.Authority,
                family.Pattern,
                family.ProviderKey,
                family.Count,
                SlotConsumption: family.Slot),
            family.Connect);

        return new FinalFamilyOutcome(DefinitionText(definition, refusedUnit), RuleLine(published.Provision), price, policy);
    }

    private Task<FinalFamilyOutcome> OutcomeOfAsync(string family) => _cache.GetAsync(family, () => BuildAsync(Catalog[family], Variants[family]));

    [Theory]
    [MemberData(nameof(Families))]
    public async Task Fxx_DEF_the_service_definition_states_its_unit_date_basis_booking_and_confirmation_and_refuses_another_quantity_unit(string family)
        => Assert.Equal(Catalog[family].Definition, (await OutcomeOfAsync(family)).Definition);

    [Theory]
    [MemberData(nameof(Families))]
    public async Task Fxx_RULE_the_published_provision_keeps_its_stage_outcome_limits_and_typed_rules(string family)
        => Assert.Equal(Catalog[family].Rules, (await OutcomeOfAsync(family)).Rule);

    [Theory]
    [MemberData(nameof(Families))]
    public async Task Fxx_PRICE_a_paid_rule_round_trips_every_rate_as_amount_and_currency_and_a_free_rule_has_no_price(string family)
        => Assert.Equal(Catalog[family].Price, (await OutcomeOfAsync(family)).Price);

    [Theory]
    [MemberData(nameof(Families))]
    public async Task Fxx_POLICY_the_inventory_authority_is_authored_without_an_invented_quota_and_is_never_a_guarantee(string family)
        => Assert.Equal(Catalog[family].Policy, (await OutcomeOfAsync(family)).Policy);

    [Fact]
    public async Task EXA_a_ten_kilogram_package_is_sold_per_point_of_sale_by_two_provisions_each_with_its_own_currencies_and_no_weight_pool()
    {
        var clock = new FixedClock();
        var harness = new InventoryHarness(_database, clock);
        var (fixture, airlineId, supplierId) = await harness.OperatorAsync(connected: false);
        var definition = await harness.ProductAsync(airlineId, supplierId, "XBAG_WEIGHT_10KG", PricingUnit.PerItem, variant: "A02");

        TestDefineProvisionCommand Package(int sequence, long pointOfSale)
            => Provision(definition.Id, sequence, coverageScope: ServiceCoverageScope.Journey, maxQuantity: 4, applicationType: ProvisionApplicationType.Baggage) with
            {
                SalesRestrictions = new(AllowedPointOfSaleIds: [pointOfSale]),
                Geography = new(AllowedRoutePairs: [Pair(Thr, Ist)]),
                AdvancePurchase = new(6, TimeUnit.Hours, MaximumPeriod: 720),
                BaggageApplication = Baggage(10m, chargeKind: BaggageChargeKind.WeightPackage, allowanceConcept: BaggageAllowanceConcept.Weight)
            };

        var german = await harness.Proof.RuleAsync(Package(10, GermanOffice), id => new TestDefinePricingRatesCommand(id, [Rate(25m, Eur), Rate(27.5m, Usd)]));
        var turkish = await harness.Proof.RuleAsync(Package(20, TurkishOffice), id => new TestDefinePricingRatesCommand(id, [Rate(7.125m, Kwd), Rate(21m, Eur)]));

        Assert.Equal(
            new[] { (10, "Active", $"POS={GermanOffice}"), (20, "Active", $"POS={TurkishOffice}") },
            new[] { german.Provision, turkish.Provision }.Select(rule => (rule.Sequence, rule.Status.Name, RuleText.Of(rule).Split("; ")[0])));
        Assert.Equal(new[] { "EUR", "USD" }, german.Pricing!.Currencies.Order());
        Assert.Equal(new[] { "EUR", "KWD" }, turkish.Pricing!.Currencies.Order());
        Assert.Equal("*[-] 25 EUR = 25 EUR | *[-] 27.5 USD = 27.5 USD", PriceText(german.Pricing));
        Assert.Equal("*[-] 21 EUR = 21 EUR | *[-] 7.125 KWD = 7.125 KWD", PriceText(turkish.Pricing));
        Assert.Equal(
            (4, 10m, "WeightPackage", "Weight", 6, 720),
            (german.Provision.MaxQuantity,
                german.Provision.BaggageApplication!.Weight!.Value,
                german.Provision.BaggageApplication.ChargeKind!.Name,
                german.Provision.BaggageApplication.AllowanceConcept!.Name,
                german.Provision.AdvancePurchase!.MinimumPeriod,
                german.Provision.AdvancePurchase.MaximumPeriod!.Value));
        Assert.Equal(
            UnlimitedPolicy,
            await PolicyTextAsync(harness, fixture, new TestDefineInventoryPolicyCommand(airlineId, "XBAG_WEIGHT_10KG", definition.Id, InventoryAuthority.Unlimited), null));
    }

    [Fact]
    public async Task EXB_fifteen_twenty_three_and_thirty_two_kilogram_pieces_are_three_definitions_with_their_own_flights_and_rates_and_no_piece_pool()
    {
        var clock = new FixedClock();
        var harness = new InventoryHarness(_database, clock);
        var (fixture, airlineId, supplierId) = await harness.OperatorAsync(connected: false);
        (int Kilograms, long FlightId, decimal Euro, decimal Dollar)[] pieces = [(15, Flight, 20m, 23m), (23, Flight, 30m, 34m), (32, OtherFlight, 45m, 52m)];
        var filed = new List<string>();

        foreach (var (kilograms, flightId, euro, dollar) in pieces)
        {
            var reference = $"XBAG_PIECE_{kilograms}KG";
            var definition = await harness.ProductAsync(airlineId, supplierId, reference, PricingUnit.PerPiece);
            var rule = await harness.Proof.RuleAsync(
                Provision(definition.Id, 10, coverageScope: ServiceCoverageScope.Journey, quantityUnit: AncillaryQuantityUnit.Piece, maxQuantity: 2, applicationType: ProvisionApplicationType.Baggage) with
                {
                    FlightApplication = new(AllowedFlightIds: [flightId]),
                    BaggageApplication = Baggage(kilograms, 1, 2)
                },
                id => new TestDefinePricingRatesCommand(id, [Rate(euro, Eur), Rate(dollar, Usd)]));

            filed.Add($"{reference} {RuleLine(rule.Provision)} | {PriceText(rule.Pricing!)}");
        }

        Assert.Equal(
            new[]
            {
                "XBAG_PIECE_15KG Both Paid Journey 1-2Piece book=False check=False | FLT=81234; BAG=1-2:15Kg:Prepaid | KIND=ExtraPiece/Piece | *[-] 20 EUR = 20 EUR | *[-] 23 USD = 23 USD",
                "XBAG_PIECE_23KG Both Paid Journey 1-2Piece book=False check=False | FLT=81234; BAG=1-2:23Kg:Prepaid | KIND=ExtraPiece/Piece | *[-] 30 EUR = 30 EUR | *[-] 34 USD = 34 USD",
                "XBAG_PIECE_32KG Both Paid Journey 1-2Piece book=False check=False | FLT=81240; BAG=1-2:32Kg:Prepaid | KIND=ExtraPiece/Piece | *[-] 45 EUR = 45 EUR | *[-] 52 USD = 52 USD"
            },
            filed);
        Assert.Equal(
            new[] { "NotConfigured", "NotConfigured", "NotConfigured" },
            new[]
            {
                (await harness.SnapshotAsync(fixture, "XBAG_PIECE_15KG", Flight)).State.Name,
                (await harness.SnapshotAsync(fixture, "XBAG_PIECE_23KG", Flight)).State.Name,
                (await harness.SnapshotAsync(fixture, "XBAG_PIECE_32KG", OtherFlight)).State.Name
            });
        Assert.Equal(NoSources, await SourceRowsAsync(harness, airlineId));
        Assert.Equal("0", (await harness.RowsAsync($"SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM Ancillary.AncillaryInventoryPolicies WHERE OwnerAirlineId = {airlineId}")).Single());
    }

    [Fact]
    public async Task EXE_a_verified_twelve_person_lounge_slot_refuses_an_overlap_accepts_an_adjacent_slot_and_records_an_immutable_adjustment_without_any_guarantee()
    {
        var clock = new FixedClock();
        var harness = new InventoryHarness(_database, clock);
        var (fixture, airlineId, supplierId) = await harness.OperatorAsync();
        var lounge = await harness.ProductAsync(airlineId, supplierId, "LOUNGE_IKA", basis: ServiceDateBasis.ServiceStart, variant: "A20");
        var policy = await harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "LOUNGE_IKA", lounge.Id, InventoryAuthority.Local, LocalInventoryPattern.AirportSlot, SlotConsumption: SlotUse())));

        Assert.Equal("16609", await OutcomeAsync(() => ActivatePolicyAsync(harness, fixture, policy.Id)));

        fixture.Airports.Add(Ika);
        fixture.Facilities![Lounge] = (Ika, "Asia/Tehran");
        await ActivatePolicyAsync(harness, fixture, policy.Id);

        var slot = await harness.RequestAsync(fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, Ika, Lounge, Utc(10), Utc(11), 12)));

        await harness.RequestAsync(fixture, scope => scope.ActivateAirportSlotInventory.ActivateAsync(new TestAirportSlotLifecycleCommand(slot.Id, 1)));
        await harness.RefusedAsync(16615, 409, fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, Ika, Lounge, Utc(10, 30), Utc(11, 30), 12)));

        var adjacent = await harness.RequestAsync(fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, Ika, Lounge, Utc(11), Utc(12), 12)));
        var configured = await harness.SnapshotAsync(fixture, "LOUNGE_IKA", atUtc: At);

        Assert.Equal((InventoryRecordStatus.Draft, Utc(11), Utc(12)), (adjacent.Status, adjacent.StartUtc, adjacent.EndUtc));
        Assert.Equal(("ConfiguredNotGuaranteed", 12, false, "NoAllocationLedger"), (configured.State.Name, configured.ConfiguredCount!.Value, configured.IsGuaranteed, configured.ReasonCode));

        await harness.RequestAsync(fixture, scope => scope.AdjustAirportSlotInventory.AdjustAsync(
            new TestAdjustAirportSlotInventoryCommand(slot.Id, 10, "FURNITURE_REMOVED", "exe-1", 2)));

        var adjusted = await harness.SnapshotAsync(fixture, "LOUNGE_IKA", atUtc: At);

        Assert.Equal(("ConfiguredNotGuaranteed", 10, false), (adjusted.State.Name, adjusted.ConfiguredCount!.Value, adjusted.IsGuaranteed));
        Assert.Equal(
            new[] { "12|10|FURNITURE_REMOVED|exe-1|2|3" },
            await harness.RowsAsync($"SELECT CONCAT(PreviousTotal, '|', NewTotal, '|', ReasonCode, '|', CorrelationId, '|', ExpectedVersion, '|', ResultingVersion) AS Value FROM Ancillary.AirportSlotAdjustments WHERE AirportSlotInventoryId = {slot.Id}"));
        Assert.Empty(await harness.SourceDifferencesAsync(airlineId));
    }
}
