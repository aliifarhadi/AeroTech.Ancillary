using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.RetireAncillaryPricing
{
    public interface IRetireAncillaryPricingService
    {
        Task<PricingResult> RetireAsync(IRetireAncillaryPricingCommand command, CancellationToken cancellationToken = default);
    }
}
