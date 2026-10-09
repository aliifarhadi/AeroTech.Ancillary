using FluentValidation;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.RetireAirportSlotInventory
{
    public abstract class RetireAirportSlotInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRetireAirportSlotInventoryCommand
    {
        protected RetireAirportSlotInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
