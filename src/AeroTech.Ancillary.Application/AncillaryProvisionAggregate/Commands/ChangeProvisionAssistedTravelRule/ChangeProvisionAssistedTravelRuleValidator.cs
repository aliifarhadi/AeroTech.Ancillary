using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAssistedTravelRule
{
    public abstract class ChangeProvisionAssistedTravelRuleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionAssistedTravelRuleCommand
    {
        protected ChangeProvisionAssistedTravelRuleValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.AssistedTravelRule!)
                .SetValidator(new ProvisionAssistedTravelRuleInputValidator())
                .When(command => command.AssistedTravelRule is not null);
        }
    }
}
