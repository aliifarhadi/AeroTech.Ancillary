namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record GroupedOfferDetailSnapshot(
        AncillaryOfferDetailSnapshot Detail,
        AncillarySelectionSnapshot? Evaluation,
        string? SelectionId,
        IReadOnlyList<string> ReasonCodes);
}
