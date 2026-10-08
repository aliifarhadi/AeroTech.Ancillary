using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing.Backoffice
{
    public sealed record BackofficeChangeAncillaryPricingCommand(
        long PricingId,
        int CurrencyId,
        FeeApplicationUnit? FeeApplicationUnit,
        IReadOnlyList<PricingLineInput> PriceLines) : IRequest<PricingResult>, IChangeAncillaryPricingCommand;
}
