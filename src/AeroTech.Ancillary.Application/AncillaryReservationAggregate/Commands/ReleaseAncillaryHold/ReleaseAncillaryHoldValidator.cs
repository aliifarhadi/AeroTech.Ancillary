using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold
{
    public abstract class ReleaseAncillaryHoldValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IReleaseAncillaryHoldCommand
    {
        protected ReleaseAncillaryHoldValidator()
        {
            RuleFor(command => command.HoldId).GreaterThan(0);
            RuleFor(command => command.UnitIds!)
                .NotEmpty()
                .Must(unitIds => unitIds.Distinct().Count() == unitIds.Count)
                .When(command => command.UnitIds is not null);
            RuleForEach(command => command.UnitIds).GreaterThan(0);
        }
    }
}
