using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V12ServiceDefinitionConformanceTests
{
    private static void ChangeUnit(AncillaryServiceDefinition definition, PricingUnit pricingUnit)
        => definition.Change(
            definition.SupplierId,
            definition.ServiceSubCode,
            definition.SubCodeSource,
            new ServiceDefinitionClassificationArgs(
                definition.ServiceTypeCode,
                definition.GroupCode,
                definition.SubGroupCode,
                definition.Description1Code,
                definition.Description2Code),
            V122Fixtures.Profile(pricingUnit, definition.ServiceDateBasis!.Value, definition.Document.Type),
            pricingUnit,
            definition.ServiceDateBasis!.Value,
            definition.CommercialName,
            definition.Description,
            DocumentDefinition.Create(definition.Document.Type, definition.Document.Rfic, definition.Document.Rfisc),
            BookingDefinition.Create(definition.Booking.Method, definition.Booking.SsrCode, definition.Booking.SsimCode),
            definition.SalesEffectiveFrom,
            definition.SalesDiscontinueOn);

    [Fact]
    public void V12_P01_the_pricing_unit_is_a_closed_set_with_explicit_values()
    {
        Assert.Equal(
            new[]
            {
                ("PerPassenger", 1), ("PerRoom", 2), ("PerItem", 3), ("PerVehicle", 4), ("PerSeat", 5), ("PerPiece", 6), ("PerKilogram", 7)
            },
            Enum.GetValues<PricingUnit>().Select(unit => (unit.ToString(), (int)unit)));
        BusinessAssert.Throws(16202, 422, () => CarrierDefinition(pricingUnit: (PricingUnit)0));
        BusinessAssert.Throws(16202, 422, () => CarrierDefinition(pricingUnit: (PricingUnit)8));
    }

    [Theory]
    [InlineData(PricingUnit.PerPassenger)]
    [InlineData(PricingUnit.PerItem)]
    [InlineData(PricingUnit.PerSeat)]
    [InlineData(PricingUnit.PerPiece)]
    [InlineData(PricingUnit.PerKilogram)]
    public void V12_P01_a_definition_carries_one_pricing_unit_and_a_revision_keeps_it(PricingUnit pricingUnit)
    {
        var definition = CarrierDefinition(pricingUnit: pricingUnit);

        Assert.Equal(pricingUnit, definition.PricingUnit);

        definition.Activate(Supplier(), Now);

        var revision = definition.Revise(1002, 2, Now.AddDays(1));

        Assert.Equal((pricingUnit, 2, ServiceDefinitionStatus.Draft), (revision.PricingUnit!.Value, revision.Version, revision.Status));
        Assert.Equal(pricingUnit, definition.PricingUnit);
    }

    [Theory]
    [InlineData(PricingUnit.PerRoom)]
    [InlineData(PricingUnit.PerVehicle)]
    public void V122_a_pricing_unit_that_no_profile_variant_sells_is_refused(PricingUnit pricingUnit)
    {
        Assert.DoesNotContain(AncillaryVariant.All, variant => variant.PricingUnits.Contains(pricingUnit));
        BusinessAssert.Throws(16202, 422, () => CarrierDefinition(pricingUnit: pricingUnit));
    }

    [Fact]
    public void V12_P01_only_a_draft_can_change_its_pricing_unit()
    {
        var definition = CarrierDefinition();

        ChangeUnit(definition, PricingUnit.PerItem);

        Assert.Equal(PricingUnit.PerItem, definition.PricingUnit);

        definition.Activate(Supplier(), Now);

        BusinessAssert.Throws(16203, 409, () => ChangeUnit(definition, PricingUnit.PerPassenger));

        definition.Suspend(Now.AddHours(1));

        BusinessAssert.Throws(16203, 409, () => ChangeUnit(definition, PricingUnit.PerPassenger));
        Assert.Equal(PricingUnit.PerItem, definition.PricingUnit);
    }

    [Fact]
    public void V12_P01_a_pricing_unit_is_assigned_once_and_never_overwritten()
    {
        var definition = CarrierDefinition(pricingUnit: PricingUnit.PerSeat);

        BusinessAssert.Throws(16211, 409, () => definition.AssignPricingUnit(PricingUnit.PerItem));
        Assert.Equal(PricingUnit.PerSeat, definition.PricingUnit);
    }
}
