using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiSelectionLineDto(
        string ClientSelectionId,
        string Kind,
        string Status,
        IReadOnlyList<string> ReasonCodes,
        bool CanPurchase,
        string? PricedSelectionRef,
        DateTimeOffset? ValidUntilUtc,
        ApiSelectionResultDataDto? Ancillary,
        OfferSeatSelectionDto? Seat);
}
