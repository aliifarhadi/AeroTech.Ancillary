using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public sealed record VerifiedSelection(
        long PricingRateId,
        ServiceCoverageScope CoverageScope,
        string TravellerRef,
        string CoverageRef,
        string OfferBinding,
        string SalesBinding,
        int Quantity,
        DateTimeOffset ExpiresAtUtc);
}
