namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto
{
    public sealed record SeatMapsDto(
        SeatMapMetaDto Meta,
        IReadOnlyList<SeatMapDto> Data,
        SeatMapDictionariesDto Dictionaries);

    public sealed record SeatMapMetaDto(int Count);

    public sealed record SeatMapDto(
        string Type,
        string FlightOfferId,
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

    public sealed record SeatMapEndpointDto(string? IataCode, string? Terminal, DateTimeOffset At);

    public sealed record SeatMapOperatingDto(string? CarrierCode);

    public sealed record SeatMapDeckDto(
        string? DeckType,
        SeatMapDeckConfigurationDto DeckConfiguration,
        IReadOnlyList<SeatMapCabinDto> Cabins);

    public sealed record SeatMapDeckConfigurationDto(
        int Width,
        int Length,
        int StartSeatRow,
        int EndSeatRow,
        int StartWingsX,
        int EndWingsX,
        int StartWingsRow,
        int EndWingsRow,
        IReadOnlyList<int> ExitRowsX);

    public sealed record SeatMapCabinDto(
        string? CabinClass,
        int? CabinClassId,
        int CabinStartSeatRow,
        int CabinEndSeatRow,
        IReadOnlyList<SeatMapItemDto> Items);

    public sealed record SeatMapItemDto(
        string? Number,
        string? Code,
        int Column,
        int Row,
        string? Title,
        IReadOnlyList<string> CharacteristicsCodes,
        IReadOnlyList<string> Conditions,
        SeatMapCoordinatesDto Coordinates,
        IReadOnlyList<SeatMapTravelerPricingDto> TravelerPricing);

    public sealed record SeatMapCoordinatesDto(int X, int Y, int Length);

    public sealed record SeatMapTravelerPricingDto(
        string? Id,
        string TravelerId,
        string SeatAvailabilityStatus,
        SeatMapPriceDto? Price);

    public sealed record SeatMapPriceDto(
        string Currency,
        string Base,
        string Total,
        IReadOnlyList<SeatMapTaxDto> Taxes,
        IReadOnlyList<SeatMapFeeDto> Fees);

    public sealed record SeatMapTaxDto(string Amount, string? Code);

    public sealed record SeatMapFeeDto(string Amount, string? Type);

    public sealed record SeatMapAvailableSeatsCounterDto(string TravelerId, int Value);

    public sealed record SeatMapDictionariesDto(
        IReadOnlyDictionary<string, string> Facilities,
        IReadOnlyDictionary<string, string> SeatCharacteristics);
}
