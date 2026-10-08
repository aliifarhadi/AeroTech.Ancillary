using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeRestriction
{
    public abstract class ChangeProvisionDayTimeRestrictionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionDayTimeRestrictionCommand
    {
        protected ChangeProvisionDayTimeRestrictionValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.RowId).GreaterThan(0);
            RuleFor(command => command.DayOfWeek).IsInEnum();
            RuleFor(command => command.Effect).IsInEnum();
        }
    }
}
