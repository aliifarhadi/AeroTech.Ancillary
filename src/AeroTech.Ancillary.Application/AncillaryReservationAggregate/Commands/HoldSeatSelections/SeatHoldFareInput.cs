using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections
{
    public sealed record SeatHoldFareInput(long AirFareId, AirFareType AirFareType, string? FareBasis, string? BookingClass);
}
