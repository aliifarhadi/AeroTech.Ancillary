using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V11HoldConformanceTests
{
    private const string QuoteProvider = "QuotePartnerA";

    private static readonly DateTimeOffset ExpiresAt = M1Fixtures.Now.AddMinutes(30);

    private static AncillaryProvision QuotedLoungeProvision()
    {
        var provision = AncillaryProvision.Define(
            501,
            M1Fixtures.LoungeDefinition(),
            100,
            ServiceCoverageScope.Sector,
            PurchaseStage.Both,
            PriceOrigin.ExternalQuote,
            QuoteProvider,
            QuantityRule.Create(AncillaryQuantityUnit.Each, 1, 1),
            ProvisionApplicationType.Standard,
            CommercialOutcome.Create(CommercialDisposition.Paid, true, false),
            SettlementDefinition.Create(ReissueRefundPolicy.NonRefundable, null, false, false),
            AvailabilityDefinition.Create(false),
            FulfillmentDefinition.Create("Ancillary"),
            P1Fixtures.Rules(null, ProvisionApplicationType.Standard),
            new Dictionary<int, int>(),
            new SequentialIdGenerator(),
            M1Fixtures.Now);

        provision.Activate(M1Fixtures.LoungeDefinition(), QuoteProvider, M1Fixtures.Now);

        return provision;
    }

    private static AncillaryReservation Hold(params AncillaryReservationUnitArgs[] units)
        => AncillaryReservation.Hold(9001, "V11-K", 12001, "ORD-12001", ExpiresAt, units, new SequentialIdGenerator(), M1Fixtures.Now);

    private static AncillaryReservationUnitArgs Overweight(BaggageSelectionArgs baggage)
        => M1Fixtures.Unit() with { VariantCode = AncillaryVariant.Overweight, Selection = new AncillaryReservationSelectionArgs(Baggage: baggage) };

    [Fact]
    public void V11_D20_a_unit_keeps_at_most_one_selection_and_only_of_its_own_family()
    {
        var bag = new BaggageSelectionArgs("B1", 27.5m);
        var overweight = Overweight(bag);

        BusinessAssert.Throws(16402, 422, () => Hold(M1Fixtures.Unit() with { Selection = new AncillaryReservationSelectionArgs(Baggage: bag) }));
        BusinessAssert.Throws(16402, 422, () => Hold(overweight with { Selection = new AncillaryReservationSelectionArgs(Baggage: bag, Meal: new MealSelectionArgs("VGML")) }));
        BusinessAssert.Throws(16402, 422, () => Hold(overweight with { VariantCode = "A99" }));
        BusinessAssert.Throws(16402, 422, () => Hold(overweight with { ServiceDefinitionRef = null }));

        var unit = Assert.Single(Hold(overweight).Units);

        Assert.Equal((AncillaryVariant.Overweight, "LNG_IKA_CIP", bag), (unit.VariantCode, unit.ServiceDefinitionRef, unit.Baggage!.ToArgs()));
        Assert.Null(unit.Meal);
    }

    [Fact]
    public void V11_D02_D04_a_selection_refuses_a_value_it_cannot_store_exactly()
    {
        BusinessAssert.Throws(16402, 422, () => Hold(Overweight(new BaggageSelectionArgs(new string('B', 65)))));
        BusinessAssert.Throws(16402, 422, () => Hold(Overweight(new BaggageSelectionArgs("B1", 23.0005m))));
        BusinessAssert.Throws(16402, 422, () => Hold(Overweight(new BaggageSelectionArgs(Dimensions: new SelectedDimensionsArgs(120m, 0m, 60m)))));
        BusinessAssert.Throws(16402, 422, () => Hold(Overweight(new BaggageSelectionArgs())));
        BusinessAssert.Throws(16402, 422, () => Hold(M1Fixtures.Unit() with
        {
            VariantCode = AncillaryVariant.Wheelchair,
            Selection = new AncillaryReservationSelectionArgs(AssistedTravel: new AssistedTravelSelectionArgs("WCH"))
        }));
    }

    [Fact]
    public void V11_L02_a_replay_compares_the_chosen_options_and_not_the_fixed_ones()
    {
        var reservation = Hold(Overweight(new BaggageSelectionArgs("B1", 27.5m, EquipmentKind: "BIKE")));
        var request = new AncillaryReservationServiceSelectionArgs(9001, 501, 71, [301], 1, new AncillaryReservationSelectionArgs(Baggage: new BaggageSelectionArgs("B1", 27.5m)));

        reservation.EnsureSameContent(12001, "ORD-12001", ExpiresAt, [request]);

        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(12001, "ORD-12001", ExpiresAt, [request with
        {
            Options = new AncillaryReservationSelectionArgs(Baggage: new BaggageSelectionArgs("B1", 28m))
        }]));
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(12001, "ORD-12001", ExpiresAt, [request with { Options = null }]));
    }

    [Fact]
    public void V11_D14_a_provision_priced_by_an_external_quote_cannot_be_held()
    {
        var refused = BusinessAssert.Throws(16419, 409, () => AncillaryHoldChecks.Accept(
            M1Fixtures.Unit(),
            M1Fixtures.ActiveLoungeDefinition(),
            QuotedLoungeProvision(),
            M1Fixtures.LocalSupplier()));

        Assert.Contains("external quote", refused.Message, StringComparison.Ordinal);

        AncillaryHoldChecks.Accept(M1Fixtures.Unit(), M1Fixtures.ActiveLoungeDefinition(), M1Fixtures.ActiveLoungeProvision(), M1Fixtures.LocalSupplier());
    }

    [Fact]
    public void V11_L01_L02_L03_a_replay_is_compared_on_the_requested_facts_in_any_order()
    {
        var reservation = AncillaryReservation.Hold(
            9001,
            "V11-K",
            12001,
            "ORD-12001",
            ExpiresAt,
            [M1Fixtures.Unit(9101), M1Fixtures.Unit(9102, travellerId: 72)],
            new SequentialIdGenerator(),
            M1Fixtures.Now);
        var first = new AncillaryReservationServiceSelectionArgs(9101, 501, 71, [301], 1, null);
        var second = new AncillaryReservationServiceSelectionArgs(9102, 501, 72, [301], null, null);

        reservation.EnsureSameContent(12001, "ORD-12001", ExpiresAt, [second, first]);

        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(12001, "ORD-12001", ExpiresAt, [second, first with { TravellerId = 73 }]));
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(12001, "ORD-12001", ExpiresAt, [second, first with { Quantity = 2 }]));
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(12001, "ORD-12001", ExpiresAt, [second, first with { ProvisionId = 502 }]));
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(12001, "ORD-12001", ExpiresAt, [first]));
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(12001, "ORD-OTHER", ExpiresAt, [second, first]));
    }
}
