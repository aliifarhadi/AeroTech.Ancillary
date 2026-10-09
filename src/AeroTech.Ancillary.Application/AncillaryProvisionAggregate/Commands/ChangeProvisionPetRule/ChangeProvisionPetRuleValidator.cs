using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPetRule
{
    public abstract class ChangeProvisionPetRuleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionPetRuleCommand
    {
        protected ChangeProvisionPetRuleValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.PetRule!)
                .SetValidator(new ProvisionPetRuleInputValidator())
                .When(command => command.PetRule is not null);
        }
    }
}
