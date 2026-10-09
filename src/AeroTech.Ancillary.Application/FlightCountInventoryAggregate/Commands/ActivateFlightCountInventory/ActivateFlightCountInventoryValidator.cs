using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.ActivateFlightCountInventory
{
    public abstract class ActivateFlightCountInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IActivateFlightCountInventoryCommand
    {
        protected ActivateFlightCountInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
