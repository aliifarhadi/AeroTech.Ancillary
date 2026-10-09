using FluentValidation;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.CloseAirportSlotInventoryForSale
{
    public abstract class CloseAirportSlotInventoryForSaleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ICloseAirportSlotInventoryForSaleCommand
    {
        protected CloseAirportSlotInventoryForSaleValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
