using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.ActivateFlightWeightInventory
{
    public abstract class ActivateFlightWeightInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IActivateFlightWeightInventoryCommand
    {
        protected ActivateFlightWeightInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
