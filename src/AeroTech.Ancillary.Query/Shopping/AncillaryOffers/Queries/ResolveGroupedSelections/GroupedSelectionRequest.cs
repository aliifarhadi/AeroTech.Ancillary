namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections
{
    public sealed record GroupedSelectionRequest(
        string? OfferId,
        long? OrderId,
        int? ExpectedOrderCommercialVersion,
        IReadOnlyList<string> Seats,
        IReadOnlyList<GroupedSelectionEntry> Selections);
}
