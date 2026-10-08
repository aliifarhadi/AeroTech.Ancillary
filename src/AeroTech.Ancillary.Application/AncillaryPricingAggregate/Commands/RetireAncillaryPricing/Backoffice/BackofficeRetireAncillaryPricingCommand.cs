using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.RetireAncillaryPricing.Backoffice
{
    public sealed record BackofficeRetireAncillaryPricingCommand(
        long PricingId) : IRequest<PricingResult>, IRetireAncillaryPricingCommand;
}
