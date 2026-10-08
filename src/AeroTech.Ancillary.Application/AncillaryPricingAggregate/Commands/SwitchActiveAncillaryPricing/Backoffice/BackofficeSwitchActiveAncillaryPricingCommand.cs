using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing.Backoffice
{
    public sealed record BackofficeSwitchActiveAncillaryPricingCommand(
        long ProvisionId,
        long NewPricingId,
        long? ExpectedOldPricingId) : IRequest<PricingResult>, ISwitchActiveAncillaryPricingCommand;
}
