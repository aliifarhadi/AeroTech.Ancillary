using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.CancelAncillaryHold
{
    public abstract class CancelAncillaryHoldValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ICancelAncillaryHoldCommand
    {
        protected CancelAncillaryHoldValidator()
        {
            RuleFor(command => command.HoldId).GreaterThan(0);
            RuleFor(command => command.ReasonCode).NotEmpty().MaximumLength(50);
        }
    }
}
