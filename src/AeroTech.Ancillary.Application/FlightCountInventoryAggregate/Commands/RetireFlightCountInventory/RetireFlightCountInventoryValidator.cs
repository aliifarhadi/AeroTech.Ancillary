using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.RetireFlightCountInventory
{
    public abstract class RetireFlightCountInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRetireFlightCountInventoryCommand
    {
        protected RetireFlightCountInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
