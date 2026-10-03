using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public sealed class ReservationFlightInputValidator : AbstractValidator<ReservationFlightInput>
    {
        public ReservationFlightInputValidator()
        {
            RuleFor(flight => flight.Ref).NotEmpty().MaximumLength(50);
            RuleFor(flight => flight.OriginAirportId).GreaterThan(0);
            RuleFor(flight => flight.DestinationAirportId).GreaterThan(0);
            RuleFor(flight => flight.DepartureDateTime).NotEmpty();
            RuleFor(flight => flight.MarketingAirlineId).GreaterThan(0);
            RuleFor(flight => flight.OperatingAirlineId).GreaterThan(0);
        }
    }
}
