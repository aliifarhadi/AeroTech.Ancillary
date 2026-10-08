using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReviseAncillaryPricing.Backoffice
{
    public sealed record BackofficeReviseAncillaryPricingCommand(
        long PricingId) : IRequest<PricingResult>, IReviseAncillaryPricingCommand;
}
