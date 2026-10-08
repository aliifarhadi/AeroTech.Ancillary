using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionPermittedTravelPeriod
{
    public abstract class RemoveProvisionPermittedTravelPeriodValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRemoveProvisionPermittedTravelPeriodCommand
    {
        protected RemoveProvisionPermittedTravelPeriodValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.RowId).GreaterThan(0);
        }
    }
}
