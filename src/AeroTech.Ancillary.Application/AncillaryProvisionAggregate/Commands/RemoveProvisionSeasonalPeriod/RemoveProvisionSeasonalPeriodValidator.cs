using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionSeasonalPeriod
{
    public abstract class RemoveProvisionSeasonalPeriodValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRemoveProvisionSeasonalPeriodCommand
    {
        protected RemoveProvisionSeasonalPeriodValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.RowId).GreaterThan(0);
        }
    }
}
