using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ActivateInventoryPolicy
{
    public abstract class ActivateInventoryPolicyValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IActivateInventoryPolicyCommand
    {
        protected ActivateInventoryPolicyValidator()
        {
            RuleFor(command => command.PolicyId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
        }
    }
}
