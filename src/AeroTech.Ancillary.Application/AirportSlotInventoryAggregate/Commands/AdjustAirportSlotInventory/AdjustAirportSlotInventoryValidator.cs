using FluentValidation;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.AdjustAirportSlotInventory
{
    public abstract class AdjustAirportSlotInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IAdjustAirportSlotInventoryCommand
    {
        protected AdjustAirportSlotInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
            RuleFor(command => command.ReasonCode).NotEmpty().MaximumLength(50);
            RuleFor(command => command.CorrelationId).NotEmpty().MaximumLength(64);
        }
    }
}
