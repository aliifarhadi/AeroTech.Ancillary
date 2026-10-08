using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod
{
    public abstract class ChangeProvisionBlackoutPeriodValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionBlackoutPeriodCommand
    {
        protected ChangeProvisionBlackoutPeriodValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.RowId).GreaterThan(0);
        }
    }
}
