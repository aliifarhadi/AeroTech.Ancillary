using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing.Backoffice
{
    public sealed record BackofficeDefineAncillaryPricingCommand(
        long AncillaryProvisionId,
        IReadOnlyList<PricingRateInput> Rates) : IRequest<PricingResult>, IDefineAncillaryPricingCommand;
}
