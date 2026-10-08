using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ActivateAncillaryPricing
{
    public interface IActivateAncillaryPricingService
    {
        Task<PricingResult> ActivateAsync(IActivateAncillaryPricingCommand command, CancellationToken cancellationToken = default);
    }
}
