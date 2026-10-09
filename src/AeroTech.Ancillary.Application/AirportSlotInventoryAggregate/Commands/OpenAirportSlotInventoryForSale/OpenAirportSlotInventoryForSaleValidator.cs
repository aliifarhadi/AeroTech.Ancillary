using FluentValidation;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.OpenAirportSlotInventoryForSale
{
    public abstract class OpenAirportSlotInventoryForSaleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IOpenAirportSlotInventoryForSaleCommand
    {
        protected OpenAirportSlotInventoryForSaleValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
