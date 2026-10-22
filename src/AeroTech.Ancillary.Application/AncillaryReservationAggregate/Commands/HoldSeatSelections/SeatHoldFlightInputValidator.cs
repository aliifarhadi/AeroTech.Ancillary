using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections
{
    public sealed class SeatHoldFlightInputValidator : AbstractValidator<SeatHoldFlightInput>
    {
        public SeatHoldFlightInputValidator()
        {
            RuleFor(flight => flight.FlightNumber).NotEmpty().MaximumLength(16);
            RuleFor(flight => flight.MarketingAirlineId).GreaterThan(0);
            RuleFor(flight => flight.OperatingAirlineId).GreaterThan(0);
            RuleFor(flight => flight.OriginAirportId).GreaterThan(0);
            RuleFor(flight => flight.DestinationAirportId).GreaterThan(0);
            RuleFor(flight => flight.AircraftId).GreaterThan(0);
            RuleFor(flight => flight.CabinClassId).GreaterThan(0);
            RuleFor(flight => flight.RbdId).GreaterThan(0).When(flight => flight.RbdId is not null);
        }
    }
}
