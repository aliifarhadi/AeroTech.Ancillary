using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.SuspendFlightCountInventory
{
    public abstract class SuspendFlightCountInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ISuspendFlightCountInventoryCommand
    {
        protected SuspendFlightCountInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
