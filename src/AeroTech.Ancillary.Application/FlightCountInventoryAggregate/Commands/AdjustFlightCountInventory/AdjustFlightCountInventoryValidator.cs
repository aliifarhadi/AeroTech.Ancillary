using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory
{
    public abstract class AdjustFlightCountInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IAdjustFlightCountInventoryCommand
    {
        protected AdjustFlightCountInventoryValidator()
        {
            RuleFor(command => command.InventoryId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
            RuleFor(command => command.ReasonCode).NotEmpty().MaximumLength(50);
            RuleFor(command => command.CorrelationId).NotEmpty().MaximumLength(64);
        }
    }
}
