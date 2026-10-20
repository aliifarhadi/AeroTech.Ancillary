namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferSeatSelectionDto(
        string SeatMapSelectionId,
        string SeatNumber,
        string TravelerId,
        string SegmentId,
        long FlightId,
        long FlightCapacityId,
        bool IncludedSeat,
        string? ServiceDefinitionRef,
        int CurrencyId,
        decimal? Total,
        OfferExactPriceDto? ExactPrice,
        bool RequiresCheckAtReserve,
        IReadOnlyList<string> ReasonCodes);
}
