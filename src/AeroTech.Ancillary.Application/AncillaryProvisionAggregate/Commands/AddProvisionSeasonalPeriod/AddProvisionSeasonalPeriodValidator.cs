using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionSeasonalPeriod
{
    public abstract class AddProvisionSeasonalPeriodValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IAddProvisionSeasonalPeriodCommand
    {
        protected AddProvisionSeasonalPeriodValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
        }
    }
}
