using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record SelectionResolutionSnapshot(long OrderId, int? OrderCommercialVersion, IReadOnlyList<ResolvedSelectionSnapshot> Results)
    {
        public bool AllValid => Results.All(result => result.Status == SelectionResolutionStatus.Valid);
    }
}
