using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionGeography
{
    public abstract class ChangeProvisionGeographyValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionGeographyCommand
    {
        protected ChangeProvisionGeographyValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.Geography!)
                .SetValidator(new ProvisionGeographyInputValidator())
                .When(command => command.Geography is not null);
        }
    }
}
