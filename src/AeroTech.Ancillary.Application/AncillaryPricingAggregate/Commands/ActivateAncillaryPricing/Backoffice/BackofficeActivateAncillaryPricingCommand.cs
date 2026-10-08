using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ActivateAncillaryPricing.Backoffice
{
    public sealed record BackofficeActivateAncillaryPricingCommand(
        long PricingId) : IRequest<PricingResult>, IActivateAncillaryPricingCommand;
}
