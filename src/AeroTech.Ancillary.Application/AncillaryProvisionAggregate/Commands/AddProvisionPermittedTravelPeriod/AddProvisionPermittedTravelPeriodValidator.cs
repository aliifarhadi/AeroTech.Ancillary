using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod
{
    public abstract class AddProvisionPermittedTravelPeriodValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IAddProvisionPermittedTravelPeriodCommand
    {
        protected AddProvisionPermittedTravelPeriodValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
        }
    }
}
