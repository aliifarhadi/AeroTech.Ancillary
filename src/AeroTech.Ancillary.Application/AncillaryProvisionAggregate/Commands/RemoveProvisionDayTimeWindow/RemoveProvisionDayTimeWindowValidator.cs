using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeWindow
{
    public abstract class RemoveProvisionDayTimeWindowValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRemoveProvisionDayTimeWindowCommand
    {
        protected RemoveProvisionDayTimeWindowValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.RowId).GreaterThan(0);
        }
    }
}
