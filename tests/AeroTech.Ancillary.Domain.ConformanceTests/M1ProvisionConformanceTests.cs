using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.M1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class M1ProvisionConformanceTests
{
    [Fact]
    public void M1_D01_a_draft_provision_carries_the_walking_shape()
    {
        var provision = LoungeProvision();

        Assert.Equal(501, provision.Id);
        Assert.Equal(1001, provision.ServiceDefinitionId);
        Assert.Equal(100, provision.Sequence);
        Assert.Equal(ProvisionStatus.Draft, provision.Status);
        Assert.Equal(ServiceCoverageScope.Sector, provision.CoverageScope);
        Assert.Equal(ProvisionApplicationType.Standard, provision.ApplicationType);
        Assert.Equal(CommercialDisposition.Paid, provision.Outcome.Disposition);
        Assert.Equal(Currency, provision.Fee!.CurrencyId);
        Assert.Equal(FeeApplicationUnit.Item, provision.Fee.ApplicationUnit);
        Assert.Equal("Ancillary", provision.Fulfillment.FulfillmentProviderKey);
        var line = Assert.Single(provision.PriceLines);
        Assert.Equal(2500000m, line.UnitAmount);
    }

    [Fact]
    public void M1_D02_the_sequence_must_be_positive()
    {
        BusinessAssert.Throws(16302, 422, () => LoungeProvision(sequence: 0));
    }

    [Fact]
    public void M1_D01_a_paid_provision_requires_a_fee_with_lines_and_other_dispositions_forbid_it()
    {
        BusinessAssert.Throws(16302, 422, () => LoungeProvision(priceLines: []));

        var free = LoungeProvision(id: 502, disposition: CommercialDisposition.Free);

        Assert.Null(free.Fee);
        Assert.Empty(free.PriceLines);

        var notAvailable = LoungeProvision(id: 503, disposition: CommercialDisposition.NotAvailable);

        Assert.Equal(CommercialDisposition.NotAvailable, notAvailable.Outcome.Disposition);
    }

    [Fact]
    public void M1_I01_a_price_line_is_a_positive_two_decimal_amount()
    {
        BusinessAssert.Throws(16302, 422, () => LoungeProvision(
            priceLines: [new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, null, 10.123m)]));
        BusinessAssert.Throws(16302, 422, () => LoungeProvision(
            priceLines: [new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, null, 0m)]));
    }

    [Fact]
    public void M1_I02_a_negative_price_line_is_refused()
    {
        BusinessAssert.Throws(16302, 422, () => LoungeProvision(
            priceLines: [new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, null, -10m)]));
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

        provision.Activate(Now.AddMinutes(1));

        Assert.Equal(ProvisionStatus.Active, provision.Status);
        Assert.Equal(Now.AddMinutes(1), provision.ActivatedAt);

        BusinessAssert.Throws(16303, 409, () => provision.Activate(Now.AddMinutes(2)));
    }

    [Fact]
    public void M1_D02_supersede_retires_only_an_active_provision()
    {
        var provision = LoungeProvision();

        BusinessAssert.Throws(16303, 409, () => provision.Supersede(Now));

        provision.Activate(Now.AddMinutes(1));
        provision.Supersede(Now.AddMinutes(2));

        Assert.Equal(ProvisionStatus.Retired, provision.Status);
        Assert.Equal(Now.AddMinutes(2), provision.RetiredAt);
    }

    [Fact]
    public void M1_G09A_an_unimplemented_fee_application_unit_cannot_be_activated()
    {
        var perKilogram = LoungeProvision(id: 504, feeApplicationUnit: FeeApplicationUnit.PerOneKilogramOver);

        BusinessAssert.Throws(16305, 422, () => perKilogram.Activate(Now.AddMinutes(1)));

        var percent = LoungeProvision(id: 505, feeApplicationUnit: FeeApplicationUnit.OnePercentOfFarePerKilogram);

        BusinessAssert.Throws(16305, 422, () => percent.Activate(Now.AddMinutes(1)));
    }
}
