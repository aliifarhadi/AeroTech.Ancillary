using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryPricingAggregate.Requests
{
    public sealed record ChangePricingRequest(IReadOnlyList<PricingRateInput> Rates);
}
