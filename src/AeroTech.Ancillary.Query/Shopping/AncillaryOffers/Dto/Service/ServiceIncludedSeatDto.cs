namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceIncludedSeatDto(
        long? OrderTravellerId,
        string TravellerRef,
        long FlightId,
        long FlightCapacityId,
        string SeatNumber,
        int CurrencyId,
        decimal Fee);
}
