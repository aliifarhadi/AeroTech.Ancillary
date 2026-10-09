using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.OpenFlightCountInventoryForSale
{
    public abstract class OpenFlightCountInventoryForSaleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IOpenFlightCountInventoryForSaleCommand
    {
        protected OpenFlightCountInventoryForSaleValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
