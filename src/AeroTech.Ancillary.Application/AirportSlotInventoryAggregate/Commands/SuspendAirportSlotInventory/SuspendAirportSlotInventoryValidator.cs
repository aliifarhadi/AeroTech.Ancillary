using FluentValidation;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.SuspendAirportSlotInventory
{
    public abstract class SuspendAirportSlotInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ISuspendAirportSlotInventoryCommand
    {
        protected SuspendAirportSlotInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
