using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeasonalPeriod
{
    public abstract class ChangeProvisionSeasonalPeriodValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionSeasonalPeriodCommand
    {
        protected ChangeProvisionSeasonalPeriodValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.RowId).GreaterThan(0);
        }
    }
}
