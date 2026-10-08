using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing
{
    public interface IChangeAncillaryPricingService
    {
        Task<PricingResult> ChangeAsync(IChangeAncillaryPricingCommand command, CancellationToken cancellationToken = default);
    }
}
