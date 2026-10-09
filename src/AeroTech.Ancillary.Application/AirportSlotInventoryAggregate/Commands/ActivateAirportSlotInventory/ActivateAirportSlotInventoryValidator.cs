using FluentValidation;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.ActivateAirportSlotInventory
{
    public abstract class ActivateAirportSlotInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IActivateAirportSlotInventoryCommand
    {
        protected ActivateAirportSlotInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
