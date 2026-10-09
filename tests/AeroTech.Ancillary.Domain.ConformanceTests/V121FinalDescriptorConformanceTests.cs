using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.V121Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V121FinalDescriptorConformanceTests
{
    private static AncillaryProvision Staged(PurchaseStage purchaseStage)
        => AncillaryProvision.Define(
            5001,
            1001,
            10,
            ServiceCoverageScope.Sector,
            purchaseStage,
            QuantityRule.Create(AncillaryQuantityUnit.Each, 1, 1),
            ProvisionApplicationType.Standard,
            CommercialOutcome.Create(CommercialDisposition.Free, false, false),
            SettlementDefinition.Create(ReissueRefundPolicy.NonRefundable, null, false, false),
            AvailabilityDefinition.Create(false),
            FulfillmentDefinition.Create("Ancillary"),
            ProvisionRulesArgs.Unrestricted,
            new SequentialIdGenerator(),
            Now);

    private static AncillaryProvision Bag(ProvisionBaggageApplicationArgs baggage)
        => Provision(Groups(baggageApplication: baggage), ProvisionApplicationType.Baggage, CommercialDisposition.Free);

    [Fact]
    public void D04_the_four_descriptor_enums_carry_exactly_the_approved_members()
    {
        Assert.Equal(new[] { "Immediate=1", "SubjectToConfirmation=2" }, Enum.GetValues<ConfirmationRequirement>().Select(value => $"{value}={(int)value}"));
        Assert.Equal(new[] { "PreOrder=1", "PostTicketed=2", "Both=3", "LegacyUnspecified=4" }, Enum.GetValues<PurchaseStage>().Select(value => $"{value}={(int)value}"));
        Assert.Equal(
            new[] { "ExtraPiece=1", "WeightPackage=2", "Overweight=3", "Oversize=4", "SpecialEquipment=5" },
            Enum.GetValues<BaggageChargeKind>().Select(value => $"{value}={(int)value}"));
        Assert.Equal(new[] { "Piece=1", "Weight=2" }, Enum.GetValues<BaggageAllowanceConcept>().Select(value => $"{value}={(int)value}"));
        Assert.DoesNotContain(typeof(ProvisionBaggageApplicationArgs).GetProperties(), property => property.Name.Contains("Step", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(PurchaseStage.PreOrder)]
    [InlineData(PurchaseStage.PostTicketed)]
    [InlineData(PurchaseStage.Both)]
    public void D04_a_provision_states_its_purchase_stage_and_keeps_it_through_publication(PurchaseStage purchaseStage)
    {
        var provision = Staged(purchaseStage);

        provision.Activate(CarrierDefinition(), Now);

        Assert.Equal((purchaseStage, ProvisionStatus.Active), (provision.PurchaseStage, provision.Status));
    }

    [Fact]
    public void D04_the_legacy_stage_is_never_authored_and_a_migrated_row_is_not_published_until_a_stage_is_stated()
    {
        BusinessAssert.Throws(16302, 422, () => Staged(PurchaseStage.LegacyUnspecified));
        BusinessAssert.Throws(16302, 422, () => Staged((PurchaseStage)9));

        var migrated = Staged(PurchaseStage.Both);

        typeof(AncillaryProvision).GetProperty(nameof(AncillaryProvision.PurchaseStage))!.SetValue(migrated, PurchaseStage.LegacyUnspecified);

        BusinessAssert.Throws(16316, 409, () => migrated.Activate(CarrierDefinition(), Now));
        Assert.Equal(ProvisionStatus.Draft, migrated.Status);

        BusinessAssert.Throws(16302, 422, () => Replace(migrated, ProvisionRulesArgs.Unrestricted, new SequentialIdGenerator()));
    }

    [Fact]
    public void D04_the_booking_states_whether_a_request_is_subject_to_confirmation_and_defaults_to_immediate()
    {
        var immediate = BookingDefinition.Create(BookingMethod.Ssr, "VGML", null);
        var requested = BookingDefinition.Create(BookingMethod.Ssr, "WCHC", null, ConfirmationRequirement.SubjectToConfirmation);

        Assert.Equal(ConfirmationRequirement.Immediate, immediate.ConfirmationRequirement);
        Assert.Equal(ConfirmationRequirement.SubjectToConfirmation, requested.ConfirmationRequirement);
        Assert.NotEqual(BookingDefinition.Create(BookingMethod.Ssr, "WCHC", null), requested);
        BusinessAssert.Throws(16202, 422, () => BookingDefinition.Create(BookingMethod.Ssr, "WCHC", null, (ConfirmationRequirement)3));
    }

    [Fact]
    public void D08_an_advance_purchase_window_has_an_optional_maximum_that_is_never_below_the_minimum()
    {
        var open = Provision(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(6, TimeUnit.Hours, false)));
        var window = Provision(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(6, TimeUnit.Hours, false, 720)));
        var exact = Provision(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(0, TimeUnit.Days, false, 0)));

        Assert.Null(open.AdvancePurchase!.MaximumPeriod);
        Assert.Equal((6, 720, TimeUnit.Hours), (window.AdvancePurchase!.MinimumPeriod, window.AdvancePurchase.MaximumPeriod!.Value, window.AdvancePurchase.Unit));
        Assert.Equal((0, 0), (exact.AdvancePurchase!.MinimumPeriod, exact.AdvancePurchase.MaximumPeriod!.Value));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(24, TimeUnit.Hours, false, 23))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(0, TimeUnit.Hours, false, -1))));

        window.Activate(CarrierDefinition(), Now);

        Assert.Equal(ProvisionStatus.Active, window.Status);
    }

    [Fact]
    public void D08_a_weight_package_is_a_weight_concept_with_its_entitlement_and_an_extra_piece_is_a_piece_concept()
    {
        var package = Bag(Baggage() with { FirstExcessPiece = null, LastExcessPiece = null, Weight = 10m, ChargeKind = BaggageChargeKind.WeightPackage, AllowanceConcept = BaggageAllowanceConcept.Weight });
        var piece = Bag(Baggage());

        Assert.Equal(
            (BaggageChargeKind.WeightPackage, BaggageAllowanceConcept.Weight, 10m),
            (package.BaggageApplication!.ChargeKind!.Value, package.BaggageApplication.AllowanceConcept!.Value, package.BaggageApplication.Weight!.Value));
        Assert.Equal((BaggageChargeKind.ExtraPiece, BaggageAllowanceConcept.Piece), (piece.BaggageApplication!.ChargeKind!.Value, piece.BaggageApplication.AllowanceConcept!.Value));
        BusinessAssert.Throws(16302, 422, () => Bag(Baggage() with { ChargeKind = BaggageChargeKind.WeightPackage, AllowanceConcept = BaggageAllowanceConcept.Piece }));
        BusinessAssert.Throws(16302, 422, () => Bag(Baggage() with { ChargeKind = BaggageChargeKind.WeightPackage, AllowanceConcept = null }));
        BusinessAssert.Throws(16302, 422, () => Bag(Baggage() with { Weight = null, ChargeKind = BaggageChargeKind.WeightPackage, AllowanceConcept = BaggageAllowanceConcept.Weight }));
        BusinessAssert.Throws(16302, 422, () => Bag(Baggage() with { ChargeKind = BaggageChargeKind.ExtraPiece, AllowanceConcept = BaggageAllowanceConcept.Weight }));
        BusinessAssert.Throws(16302, 422, () => Bag(Baggage() with { ChargeKind = BaggageChargeKind.ExtraPiece, AllowanceConcept = null }));
        BusinessAssert.Throws(16302, 422, () => Bag(Baggage() with { ChargeKind = (BaggageChargeKind)6 }));
        BusinessAssert.Throws(16302, 422, () => Bag(Baggage() with { AllowanceConcept = (BaggageAllowanceConcept)3 }));
    }

    [Theory]
    [InlineData(BaggageChargeKind.Overweight, null)]
    [InlineData(BaggageChargeKind.Oversize, null)]
    [InlineData(BaggageChargeKind.SpecialEquipment, null)]
    [InlineData(BaggageChargeKind.Overweight, BaggageAllowanceConcept.Weight)]
    [InlineData(BaggageChargeKind.SpecialEquipment, BaggageAllowanceConcept.Piece)]
    public void D08_overweight_oversize_and_special_equipment_take_the_concept_only_when_it_is_sourced(BaggageChargeKind chargeKind, BaggageAllowanceConcept? allowanceConcept)
    {
        var provision = Bag(Baggage() with { ChargeKind = chargeKind, AllowanceConcept = allowanceConcept });

        provision.Activate(CarrierDefinition(), Now);

        Assert.Equal((chargeKind, allowanceConcept, ProvisionStatus.Active), (provision.BaggageApplication!.ChargeKind!.Value, provision.BaggageApplication.AllowanceConcept, provision.Status));
    }

    [Fact]
    public void D08_a_baggage_rule_without_a_charge_kind_stays_a_draft()
    {
        var legacy = Bag(Baggage() with { ChargeKind = null, AllowanceConcept = null });

        BusinessAssert.Throws(16317, 409, () => legacy.Activate(CarrierDefinition(), Now));
        Assert.Equal(ProvisionStatus.Draft, legacy.Status);
    }
}
