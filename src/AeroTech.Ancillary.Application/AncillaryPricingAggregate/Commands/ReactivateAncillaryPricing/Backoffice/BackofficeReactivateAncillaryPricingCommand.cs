using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReactivateAncillaryPricing.Backoffice
{
    public sealed record BackofficeReactivateAncillaryPricingCommand(
        long PricingId) : IRequest<PricingResult>, IReactivateAncillaryPricingCommand;
}
