using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.AdjustFlightWeightInventory
{
    public abstract class AdjustFlightWeightInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IAdjustFlightWeightInventoryCommand
    {
        protected AdjustFlightWeightInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
            RuleFor(command => command.ReasonCode).NotEmpty().MaximumLength(50);
            RuleFor(command => command.CorrelationId).NotEmpty().MaximumLength(64);
        }
    }
}
