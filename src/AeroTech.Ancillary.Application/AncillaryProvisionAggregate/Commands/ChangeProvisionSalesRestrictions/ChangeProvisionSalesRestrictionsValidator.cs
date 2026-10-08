using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSalesRestrictions
{
    public abstract class ChangeProvisionSalesRestrictionsValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionSalesRestrictionsCommand
    {
        protected ChangeProvisionSalesRestrictionsValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.SalesRestrictions!)
                .SetValidator(new ProvisionSalesRestrictionsInputValidator())
                .When(command => command.SalesRestrictions is not null);
        }
    }
}
