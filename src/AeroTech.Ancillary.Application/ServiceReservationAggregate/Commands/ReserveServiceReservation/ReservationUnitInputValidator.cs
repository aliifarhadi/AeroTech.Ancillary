using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public sealed class ReservationUnitInputValidator : AbstractValidator<ReservationUnitInput>
    {
        public ReservationUnitInputValidator()
        {
            RuleFor(unit => unit.UnitReference).NotEmpty().MaximumLength(128);
            RuleFor(unit => unit.ProductRef).NotEmpty().Matches("^[A-Z0-9]{2,20}$");
            RuleFor(unit => unit.ProductVersion).GreaterThan(0);
            RuleFor(unit => unit.TravellerRef).NotEmpty();
            RuleFor(unit => unit.Quantity).GreaterThan(0);
        }
    }
}
