using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public sealed class ReservationExistingOccurrenceInputValidator : AbstractValidator<ReservationExistingOccurrenceInput>
    {
        public ReservationExistingOccurrenceInputValidator()
        {
            RuleFor(existing => existing.ProductRef).NotEmpty();
            RuleFor(existing => existing.TravellerRef).NotEmpty();
            RuleFor(existing => existing.Quantity).GreaterThan(0);
        }
    }
}
