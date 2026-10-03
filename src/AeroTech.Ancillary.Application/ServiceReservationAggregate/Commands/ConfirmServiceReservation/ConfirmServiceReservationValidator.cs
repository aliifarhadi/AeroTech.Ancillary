using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ConfirmServiceReservation
{
    public abstract class ConfirmServiceReservationValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IConfirmServiceReservationCommand
    {
        protected ConfirmServiceReservationValidator()
        {
            RuleFor(command => command.ServiceReservationId).GreaterThan(0);
        }
    }
}
