using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing
{
    public interface ISwitchActiveAncillaryPricingService
    {
        Task<PricingResult> SwitchAsync(ISwitchActiveAncillaryPricingCommand command, CancellationToken cancellationToken = default);
    }
}
