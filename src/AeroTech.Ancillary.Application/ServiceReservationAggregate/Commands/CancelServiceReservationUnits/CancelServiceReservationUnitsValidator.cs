using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.CancelServiceReservationUnits
{
    public abstract class CancelServiceReservationUnitsValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ICancelServiceReservationUnitsCommand
    {
        protected CancelServiceReservationUnitsValidator()
        {
            RuleFor(command => command.ServiceReservationId).GreaterThan(0);
            RuleFor(command => command.UnitRefs).NotNull();
        }
    }
}
