using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public abstract class ReserveServiceReservationValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IReserveServiceReservationCommand
    {
        protected ReserveServiceReservationValidator()
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(128);
            RuleFor(command => command.Reference).NotEmpty().MaximumLength(128);
            RuleFor(command => command.ExpiresAt).NotEmpty();
            RuleFor(command => command.Context).NotNull().SetValidator(new ReservationContextInputValidator());
            RuleFor(command => command.Units).NotNull();
            RuleForEach(command => command.Units).NotNull().SetValidator(new ReservationUnitInputValidator());
        }
    }
}
