using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Arguments
{
    public sealed record SelectionSnapshotLineArgs(
        string OfferItemId,
        long PricingRateId,
        ServiceCoverageScope CoverageScope,
        string? TravellerRef,
        string CoverageRef,
        string SourceBinding,
        string SalesBinding,
        int Quantity,
        string? SelectedJson);
}
