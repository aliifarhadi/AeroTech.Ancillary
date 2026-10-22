using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers
{
    public interface IGroupedSelectionIdProvider
    {
        Task<(AncillarySelectionSnapshot? Evaluation, string? SelectionId)> IssueAsync(
            AncillaryShoppingSnapshot shopping,
            AncillaryOfferItem item,
            long? orderId,
            CancellationToken cancellationToken = default);

        (AncillarySelectionSnapshot Evaluation, string? SelectionId) Issue(AncillarySelectionSnapshot evaluation, long? orderId);
    }
}
