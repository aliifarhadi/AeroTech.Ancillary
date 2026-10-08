namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing
{
    public interface ISwitchActiveAncillaryPricingCommand
    {
        long ProvisionId { get; }

        long NewPricingId { get; }

        long? ExpectedOldPricingId { get; }
    }
}
