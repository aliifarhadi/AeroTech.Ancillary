using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.CloseFlightWeightInventoryForSale
{
    public abstract class CloseFlightWeightInventoryForSaleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ICloseFlightWeightInventoryForSaleCommand
    {
        protected CloseFlightWeightInventoryForSaleValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
