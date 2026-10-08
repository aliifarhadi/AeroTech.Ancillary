using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.M1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class M1ProvisionConformanceTests
{
    private static AncillaryPricingLineArgs Line(decimal amount)
        => new(null, null, null, AncillaryPriceLineCategory.Ancillary, null, null, null, null, amount);

    [Fact]
    public void M1_D01_a_draft_provision_carries_the_walking_shape()
    {
        var provision = LoungeProvision();
        var pricing = LoungePricing();

        Assert.Equal(501, provision.Id);
        Assert.Equal(1001, provision.ServiceDefinitionId);
        Assert.Equal(100, provision.Sequence);
        Assert.Equal(ProvisionStatus.Draft, provision.Status);
        Assert.Equal(ServiceCoverageScope.Sector, provision.CoverageScope);
        Assert.Equal(ProvisionApplicationType.Standard, provision.ApplicationType);
        Assert.Equal(CommercialDisposition.Paid, provision.Outcome.Disposition);
        Assert.Equal("Ancillary", provision.Fulfillment.FulfillmentProviderKey);
        Assert.Equal((501L, Currency, FeeApplicationUnit.Item, PricingStatus.Draft), (pricing.AncillaryProvisionId, pricing.CurrencyId, pricing.FeeApplicationUnit!.Value, pricing.Status));
        Assert.Equal(2500000m, Assert.Single(pricing.PriceLines).Amount);
    }

    [Fact]
    public void M1_D02_the_sequence_must_be_positive()
    {
        BusinessAssert.Throws(16302, 422, () => LoungeProvision(sequence: 0));
    }

    [Fact]
    public void M1_D01_a_paid_price_requires_a_base_line_and_other_dispositions_are_authored_without_money()
    {
        BusinessAssert.Throws(16508, 409, () => LoungePricing(priceLines: []));

        var free = LoungeProvision(id: 502, disposition: CommercialDisposition.Free);

        Assert.Equal(CommercialDisposition.Free, free.Outcome.Disposition);

        var notAvailable = LoungeProvision(id: 503, disposition: CommercialDisposition.NotAvailable);

        Assert.Equal(CommercialDisposition.NotAvailable, notAvailable.Outcome.Disposition);
    }

    [Fact]
    public void M1_I01_a_price_line_is_a_positive_two_decimal_amount()
    {
        BusinessAssert.Throws(16502, 422, () => LoungePricing(priceLines: [Line(10.123m)]));
        BusinessAssert.Throws(16502, 422, () => LoungePricing(priceLines: [Line(0m)]));
    }

    [Fact]
    public void M1_I02_a_negative_price_line_is_refused()
    {
        BusinessAssert.Throws(16502, 422, () => LoungePricing(priceLines: [Line(-10m)]));
    }

    [Fact]
    public void M1_D01_the_quantity_rule_is_validated()
    {
        BusinessAssert.Throws(16302, 422, () => QuantityRule.Create(AncillaryQuantityUnit.Each, 0, 1));
        BusinessAssert.Throws(16302, 422, () => QuantityRule.Create(AncillaryQuantityUnit.Each, 2, 1));
    }

    [Fact]
    public void M1_D02_activation_moves_a_draft_to_active_once()
    {
        var provision = LoungeProvision();

        provision.Activate(LoungeDefinition(), Now.AddMinutes(1));

        Assert.Equal(ProvisionStatus.Active, provision.Status);
        Assert.Equal(Now.AddMinutes(1), provision.ActivatedAt);
        BusinessAssert.Throws(16303, 409, () => provision.Activate(LoungeDefinition(), Now.AddMinutes(2)));
    }

    [Fact]
    public void M1_D02_supersede_retires_only_an_active_provision()
    {
        var provision = LoungeProvision();

        BusinessAssert.Throws(16303, 409, () => provision.Supersede(Now));

        provision.Activate(LoungeDefinition(), Now.AddMinutes(1));
        provision.Supersede(Now.AddMinutes(2));

        Assert.Equal(ProvisionStatus.Retired, provision.Status);
        Assert.Equal(Now.AddMinutes(2), provision.RetiredAt);
    }

    [Fact]
    public void M1_G09A_an_unimplemented_fee_application_unit_cannot_be_activated()
    {
        var perKilogram = LoungePricing(id: 704, feeApplicationUnit: FeeApplicationUnit.PerOneKilogramOver);

        BusinessAssert.Throws(16305, 422, () => perKilogram.Activate(Now.AddMinutes(1)));

        var percent = LoungePricing(id: 705, feeApplicationUnit: FeeApplicationUnit.OnePercentOfFarePerKilogram);

        BusinessAssert.Throws(16305, 422, () => percent.Activate(Now.AddMinutes(1)));
    }
}
