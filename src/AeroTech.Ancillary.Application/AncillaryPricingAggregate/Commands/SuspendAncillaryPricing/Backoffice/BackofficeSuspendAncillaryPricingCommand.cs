using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SuspendAncillaryPricing.Backoffice
{
    public sealed record BackofficeSuspendAncillaryPricingCommand(
        long PricingId) : IRequest<PricingResult>, ISuspendAncillaryPricingCommand;
}
