using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.SuspendInventoryPolicy
{
    public abstract class SuspendInventoryPolicyValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ISuspendInventoryPolicyCommand
    {
        protected SuspendInventoryPolicyValidator()
        {
            RuleFor(command => command.PolicyId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
