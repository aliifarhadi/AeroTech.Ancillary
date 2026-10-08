using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeRestriction
{
    public abstract class AddProvisionDayTimeRestrictionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IAddProvisionDayTimeRestrictionCommand
    {
        protected AddProvisionDayTimeRestrictionValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.DayOfWeek).IsInEnum();
            RuleFor(command => command.Effect).IsInEnum();
        }
    }
}
