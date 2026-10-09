using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing
{
    public interface IChangeAncillaryPricingCommand
    {
        long PricingId { get; }

        IReadOnlyList<PricingRateInput> Rates { get; }
    }
}
