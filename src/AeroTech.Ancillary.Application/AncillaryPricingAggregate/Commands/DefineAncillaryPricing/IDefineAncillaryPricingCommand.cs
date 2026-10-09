namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    public interface IDefineAncillaryPricingCommand
    {
        long AncillaryProvisionId { get; }

        IReadOnlyList<PricingRateInput> Rates { get; }
    }
}
