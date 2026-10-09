using FluentValidation;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory
{
    public abstract class DefineFlightCountInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IDefineFlightCountInventoryCommand
    {
        protected DefineFlightCountInventoryValidator()
        {
            RuleFor(command => command.OwnerAirlineId).GreaterThan(0);
            RuleFor(command => command.FlightId).GreaterThan(0);
            RuleFor(command => command.ResourceId).GreaterThan(0);
            RuleFor(command => command.CountUnit).IsInEnum();
        }
    }
}
