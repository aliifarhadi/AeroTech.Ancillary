using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public sealed class ReservationTravellerInputValidator : AbstractValidator<ReservationTravellerInput>
    {
        public ReservationTravellerInputValidator()
        {
            RuleFor(traveller => traveller.Ref).NotEmpty().MaximumLength(50);
            RuleFor(traveller => traveller.PassengerTypeCode).NotEmpty();
            RuleFor(traveller => traveller.FlightRefs).NotEmpty();
            RuleForEach(traveller => traveller.FlightRefs).NotEmpty();
        }
    }
}
