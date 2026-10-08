using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod
{
    public abstract class RemoveProvisionBlackoutPeriodValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRemoveProvisionBlackoutPeriodCommand
    {
        protected RemoveProvisionBlackoutPeriodValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.RowId).GreaterThan(0);
        }
    }
}
