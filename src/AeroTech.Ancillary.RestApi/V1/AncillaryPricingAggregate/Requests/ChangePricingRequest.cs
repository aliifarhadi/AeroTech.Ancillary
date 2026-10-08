using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryPricingAggregate.Requests
{
    public sealed record ChangePricingRequest(
        int CurrencyId,
        FeeApplicationUnit? FeeApplicationUnit,
        IReadOnlyList<PricingLineInput> PriceLines);
}
