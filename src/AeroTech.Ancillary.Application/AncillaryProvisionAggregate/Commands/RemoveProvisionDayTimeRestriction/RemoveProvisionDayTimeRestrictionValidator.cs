using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeRestriction
{
    public abstract class RemoveProvisionDayTimeRestrictionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRemoveProvisionDayTimeRestrictionCommand
    {
        protected RemoveProvisionDayTimeRestrictionValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.RowId).GreaterThan(0);
        }
    }
}
