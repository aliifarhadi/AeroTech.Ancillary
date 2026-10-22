using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold
{
    public abstract class ReleaseAncillaryHoldValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IReleaseAncillaryHoldCommand
    {
        protected ReleaseAncillaryHoldValidator()
        {
            RuleFor(command => command.HoldId).GreaterThan(0);
        }
    }
}
