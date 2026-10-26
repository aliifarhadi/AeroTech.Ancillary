namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments
{
    public sealed record AncillaryReservationSeatSelectionArgs(
        long OrderServiceId,
        long TravellerId,
        string SeatMapSelectionId);
}
