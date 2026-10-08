using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAdvancePurchase
{
    public abstract class ChangeProvisionAdvancePurchaseValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionAdvancePurchaseCommand
    {
        protected ChangeProvisionAdvancePurchaseValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.AdvancePurchase!)
                .SetValidator(new ProvisionAdvancePurchaseInputValidator())
                .When(command => command.AdvancePurchase is not null);
        }
    }
}
