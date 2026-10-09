using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    public sealed record PricingResult(
        long Id,
        long AncillaryProvisionId,
        int Version,
        PricingUnit? PricingUnit,
        IReadOnlyList<int> CurrencyIds,
        PricingStatus Status);
}
