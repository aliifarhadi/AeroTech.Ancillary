using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory
{
    public abstract class DefineFlightWeightInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IDefineFlightWeightInventoryCommand
    {
        protected DefineFlightWeightInventoryValidator()
        {
            RuleFor(command => command.OwnerAirlineId).GreaterThan(0);
            RuleFor(command => command.FlightId).GreaterThan(0);
            RuleFor(command => command.WeightResourceId).GreaterThan(0);
        }
    }
}
