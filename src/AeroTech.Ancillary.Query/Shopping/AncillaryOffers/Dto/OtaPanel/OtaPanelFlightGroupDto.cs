namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelFlightGroupDto(
        string FlightRef,
        string PortionRef,
        string? FlightNumber,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset DepartureAt,
        OtaPanelIncludedBaggageDto? IncludedCheckedBaggage,
        OtaPanelIncludedBaggageDto? IncludedCabinBaggage,
        IReadOnlyList<OtaPanelServiceDto> Services);
}
