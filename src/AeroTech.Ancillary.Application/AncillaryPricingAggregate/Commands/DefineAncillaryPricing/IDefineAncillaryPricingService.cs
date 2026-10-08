namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    public interface IDefineAncillaryPricingService
    {
        Task<PricingResult> DefineAsync(IDefineAncillaryPricingCommand command, CancellationToken cancellationToken = default);
    }
}
