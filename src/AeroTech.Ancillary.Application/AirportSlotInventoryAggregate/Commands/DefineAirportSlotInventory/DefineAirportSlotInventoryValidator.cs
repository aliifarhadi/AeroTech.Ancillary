using FluentValidation;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory
{
    public abstract class DefineAirportSlotInventoryValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IDefineAirportSlotInventoryCommand
    {
        protected DefineAirportSlotInventoryValidator()
        {
            RuleFor(command => command.OwnerAirlineId).GreaterThan(0);
            RuleFor(command => command.AirportId).GreaterThan(0);
            RuleFor(command => command.FacilityId).GreaterThan(0);
        }
    }
}
