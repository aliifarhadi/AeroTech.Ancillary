using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public sealed class ReservationBoundInputValidator : AbstractValidator<ReservationBoundInput>
    {
        public ReservationBoundInputValidator()
        {
            RuleFor(bound => bound.Ref).NotEmpty().MaximumLength(50);
            RuleFor(bound => bound.Flights).NotEmpty();
            RuleForEach(bound => bound.Flights).NotNull().SetValidator(new ReservationFlightInputValidator());
        }
    }
}
