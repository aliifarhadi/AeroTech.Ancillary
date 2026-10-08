using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing.Backoffice
{
    public sealed record BackofficeDefineAncillaryPricingCommand(
        long AncillaryProvisionId,
        int CurrencyId,
        FeeApplicationUnit? FeeApplicationUnit,
        IReadOnlyList<PricingLineInput> PriceLines) : IRequest<PricingResult>, IDefineAncillaryPricingCommand;
}
