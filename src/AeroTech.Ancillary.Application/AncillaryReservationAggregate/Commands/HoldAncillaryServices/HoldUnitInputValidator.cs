using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class HoldUnitInputValidator : AbstractValidator<HoldUnitInput>
    {
        public HoldUnitInputValidator()
        {
            RuleFor(unit => unit.TravellerId).GreaterThan(0);
        }
    }
}
