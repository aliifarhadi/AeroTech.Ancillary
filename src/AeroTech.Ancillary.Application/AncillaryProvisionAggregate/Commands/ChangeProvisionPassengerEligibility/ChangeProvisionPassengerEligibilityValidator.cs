using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPassengerEligibility
{
    public abstract class ChangeProvisionPassengerEligibilityValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionPassengerEligibilityCommand
    {
        protected ChangeProvisionPassengerEligibilityValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.PassengerEligibility!)
                .SetValidator(new ProvisionPassengerEligibilityInputValidator())
                .When(command => command.PassengerEligibility is not null);
        }
    }
}
