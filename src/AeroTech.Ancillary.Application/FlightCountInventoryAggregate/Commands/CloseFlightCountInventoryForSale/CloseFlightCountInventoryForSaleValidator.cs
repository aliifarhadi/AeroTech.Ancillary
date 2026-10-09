using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.CloseFlightCountInventoryForSale
{
    public abstract class CloseFlightCountInventoryForSaleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ICloseFlightCountInventoryForSaleCommand
    {
        protected CloseFlightCountInventoryForSaleValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
