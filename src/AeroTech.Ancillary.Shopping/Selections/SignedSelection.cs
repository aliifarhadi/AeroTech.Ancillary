using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public sealed record SignedSelection(
        SelectionIdKind Kind,
        long PricingRateId,
        ServiceCoverageScope CoverageScope,
        string TravellerRef,
        string CoverageRef,
        int? MinQuantity,
        int? MaxQuantity,
        string SourceBinding,
        string SalesBinding,
        DateTimeOffset ExpiresAtUtc);
}
