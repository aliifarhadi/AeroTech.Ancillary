using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Options
{
    public sealed record TypedSelectionProof(
        long PricingRateId,
        ServiceCoverageScope CoverageScope,
        string TravellerRef,
        string CoverageRef,
        string OfferBinding,
        string SalesBinding,
        int Quantity,
        DateTimeOffset ExpiresAtUtc);
}
