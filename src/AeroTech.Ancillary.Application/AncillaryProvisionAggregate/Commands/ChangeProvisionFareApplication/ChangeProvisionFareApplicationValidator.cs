using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFareApplication
{
    public abstract class ChangeProvisionFareApplicationValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionFareApplicationCommand
    {
        protected ChangeProvisionFareApplicationValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.FareApplication!)
                .SetValidator(new ProvisionFareApplicationInputValidator())
                .When(command => command.FareApplication is not null);
        }
    }
}
