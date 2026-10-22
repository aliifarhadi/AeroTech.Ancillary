using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections
{
    public sealed record SeatHoldSelectionInput(
        long OrderServiceId,
        long TravellerId,
        string SeatMapSelectionId,
        PassengerTypeCode PassengerTypeCode,
        DateOnly? DateOfBirth,
        SeatHoldFlightInput Flight,
        SeatHoldFareInput? Fare,
        bool? CouponOpenForUse = null);
}
