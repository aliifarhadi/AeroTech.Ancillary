using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.SuspendFlightWeightInventory
{
    public abstract class SuspendFlightWeightInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ISuspendFlightWeightInventoryCommand
    {
        protected SuspendFlightWeightInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
