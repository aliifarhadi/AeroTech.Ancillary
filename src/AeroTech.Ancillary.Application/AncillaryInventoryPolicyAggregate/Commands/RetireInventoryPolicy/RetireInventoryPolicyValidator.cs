using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.RetireInventoryPolicy
{
    public abstract class RetireInventoryPolicyValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRetireInventoryPolicyCommand
    {
        protected RetireInventoryPolicyValidator()
        {
            RuleFor(command => command.PolicyId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
