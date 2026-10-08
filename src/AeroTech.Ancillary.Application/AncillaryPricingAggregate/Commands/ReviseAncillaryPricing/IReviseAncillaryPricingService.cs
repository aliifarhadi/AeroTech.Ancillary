using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReviseAncillaryPricing
{
    public interface IReviseAncillaryPricingService
    {
        Task<PricingResult> ReviseAsync(IReviseAncillaryPricingCommand command, CancellationToken cancellationToken = default);
    }
}
