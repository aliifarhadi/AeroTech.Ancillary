using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.RetireFlightWeightInventory
{
    public abstract class RetireFlightWeightInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRetireFlightWeightInventoryCommand
    {
        protected RetireFlightWeightInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
