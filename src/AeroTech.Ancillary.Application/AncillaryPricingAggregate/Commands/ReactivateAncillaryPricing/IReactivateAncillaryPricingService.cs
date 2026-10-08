using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReactivateAncillaryPricing
{
    public interface IReactivateAncillaryPricingService
    {
        Task<PricingResult> ReactivateAsync(IReactivateAncillaryPricingCommand command, CancellationToken cancellationToken = default);
    }
}
