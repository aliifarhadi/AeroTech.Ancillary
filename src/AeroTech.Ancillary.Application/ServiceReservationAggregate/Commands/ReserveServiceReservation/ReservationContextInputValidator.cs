using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public sealed class ReservationContextInputValidator : AbstractValidator<ReservationContextInput>
    {
        public ReservationContextInputValidator()
        {
            RuleFor(context => context.CurrencyId).GreaterThan(0);
            RuleFor(context => context.AsOf).NotEmpty();
            RuleFor(context => context.Travellers).NotEmpty();
            RuleForEach(context => context.Travellers).NotNull().SetValidator(new ReservationTravellerInputValidator());
            RuleFor(context => context.Bounds).NotEmpty();
            RuleForEach(context => context.Bounds).NotNull().SetValidator(new ReservationBoundInputValidator());
            RuleForEach(context => context.Existing).NotNull().SetValidator(new ReservationExistingOccurrenceInputValidator());
        }
    }
}
