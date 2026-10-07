using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold
{
    public abstract class ConfirmAncillaryHoldValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IConfirmAncillaryHoldCommand
    {
        protected ConfirmAncillaryHoldValidator()
        {
            RuleFor(command => command.HoldId).GreaterThan(0);
        }
    }
}
