using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class AncillaryUnitHoldInputValidator<TSelected> : AbstractValidator<AncillaryUnitHoldInput<TSelected>>
        where TSelected : class
    {
        public AncillaryUnitHoldInputValidator(IValidator<TSelected> selected)
        {
            RuleFor(unit => unit.OrderServiceId).GreaterThan(0);
            RuleFor(unit => unit.Unit!).SetValidator(new HoldUnitInputValidator()).When(unit => unit.Unit is not null);
            RuleFor(unit => unit.Coverage!).SetValidator(new HoldCoverageInputValidator()).When(unit => unit.Coverage is not null);
            RuleFor(unit => unit.Selected!).SetValidator(selected).When(unit => unit.Selected is not null);
        }
    }
}
