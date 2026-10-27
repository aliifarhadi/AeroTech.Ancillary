namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto
{
    public sealed record OrderSeatMapsDto(
        SeatMapMetaDto Meta,
        IReadOnlyList<OrderSeatMapDto> Data,
        SeatMapDictionariesDto Dictionaries);

    public sealed record OrderSeatMapDto(
        string Type,
        long OrderId,
        string SegmentId,
        SeatMapEndpointDto Departure,
        SeatMapEndpointDto Arrival,
        string? CarrierCode,
        string? Number,
        SeatMapOperatingDto Operating,
        int AircraftId,
        string? Class,
        int? CabinClassId,
        IReadOnlyList<SeatMapDeckDto> Decks,
        IReadOnlyList<SeatMapAvailableSeatsCounterDto> AvailableSeatsCounters);
}
