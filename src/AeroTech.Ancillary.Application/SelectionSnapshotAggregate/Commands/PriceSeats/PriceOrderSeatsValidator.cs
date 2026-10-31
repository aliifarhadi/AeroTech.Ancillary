using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats
{
    public abstract class PriceOrderSeatsValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IPriceOrderSeatsCommand
    {
        protected PriceOrderSeatsValidator()
        {
            RuleFor(command => command.OrderId).NotNull().GreaterThan(0);
            Include(new PriceSeatsValidator<TCommand>());
        }
    }
}
