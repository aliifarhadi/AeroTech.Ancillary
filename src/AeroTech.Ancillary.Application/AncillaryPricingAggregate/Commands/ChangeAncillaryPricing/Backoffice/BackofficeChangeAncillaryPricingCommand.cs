using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing.Backoffice
{
    public sealed record BackofficeChangeAncillaryPricingCommand(
        long PricingId,
        IReadOnlyList<PricingRateInput> Rates) : IRequest<PricingResult>, IChangeAncillaryPricingCommand;
}
