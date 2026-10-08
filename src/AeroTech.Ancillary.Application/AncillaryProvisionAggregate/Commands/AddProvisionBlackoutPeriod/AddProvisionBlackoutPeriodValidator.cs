using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod
{
    public abstract class AddProvisionBlackoutPeriodValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IAddProvisionBlackoutPeriodCommand
    {
        protected AddProvisionBlackoutPeriodValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
        }
    }
}
