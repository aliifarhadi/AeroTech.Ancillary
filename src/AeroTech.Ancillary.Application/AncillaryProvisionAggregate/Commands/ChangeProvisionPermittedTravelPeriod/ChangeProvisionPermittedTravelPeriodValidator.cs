using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPermittedTravelPeriod
{
    public abstract class ChangeProvisionPermittedTravelPeriodValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionPermittedTravelPeriodCommand
    {
        protected ChangeProvisionPermittedTravelPeriodValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.RowId).GreaterThan(0);
        }
    }
}
