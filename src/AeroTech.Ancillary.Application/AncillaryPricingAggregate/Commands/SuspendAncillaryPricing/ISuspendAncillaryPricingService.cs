using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SuspendAncillaryPricing
{
    public interface ISuspendAncillaryPricingService
    {
        Task<PricingResult> SuspendAsync(ISuspendAncillaryPricingCommand command, CancellationToken cancellationToken = default);
    }
}
