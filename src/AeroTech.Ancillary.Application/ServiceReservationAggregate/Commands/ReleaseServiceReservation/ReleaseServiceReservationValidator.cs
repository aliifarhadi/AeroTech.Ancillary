using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReleaseServiceReservation
{
    public abstract class ReleaseServiceReservationValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IReleaseServiceReservationCommand
    {
        protected ReleaseServiceReservationValidator()
        {
            RuleFor(command => command.ServiceReservationId).GreaterThan(0);
        }
    }
}
