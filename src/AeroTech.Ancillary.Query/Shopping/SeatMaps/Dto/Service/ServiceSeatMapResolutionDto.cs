namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatMapResolutionDto(long? OrderId, int? OrderCommercialVersion, IReadOnlyList<ServiceResolvedSeatDto> Seats);
}
