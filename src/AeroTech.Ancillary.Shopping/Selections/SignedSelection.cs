using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public sealed record SignedSelection(
        SelectionIdKind Kind,
        long PricingRateId,
        ServiceCoverageScope CoverageScope,
        string TravellerRef,
        string CoverageRef,
        int? Quantity,
        string? OfferId,
        string OfferBinding,
        string SalesBinding,
        DateTimeOffset ExpiresAtUtc);
}
