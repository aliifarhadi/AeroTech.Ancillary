using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections
{
    public sealed record GroupedSelectionQuery(string? OfferId, long? OrderId) : IAncillaryOffersQuery;
}
