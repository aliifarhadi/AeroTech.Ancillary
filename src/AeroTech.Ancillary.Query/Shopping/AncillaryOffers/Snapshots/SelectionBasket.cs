using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record SelectionBasket(
        bool AllReady,
        int CurrencyId,
        decimal? Total,
        IReadOnlyList<PriceComponentLine> SharedFees,
        IReadOnlyList<string> ReasonCodes);
}
