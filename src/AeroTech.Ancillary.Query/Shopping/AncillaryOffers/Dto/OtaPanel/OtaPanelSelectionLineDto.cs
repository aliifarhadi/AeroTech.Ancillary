using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelSelectionLineDto(
        string ClientSelectionId,
        string Kind,
        string Status,
        IReadOnlyList<string> ReasonCodes,
        bool CanPurchase,
        string? PricedSelectionRef,
        DateTimeOffset? ValidUntilUtc,
        OtaPanelSelectionResultDto? Ancillary,
        OfferSeatSelectionDto? Seat);
}
