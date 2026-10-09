using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.OpenFlightWeightInventoryForSale
{
    public abstract class OpenFlightWeightInventoryForSaleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IOpenFlightWeightInventoryForSaleCommand
    {
        protected OpenFlightWeightInventoryForSaleValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
