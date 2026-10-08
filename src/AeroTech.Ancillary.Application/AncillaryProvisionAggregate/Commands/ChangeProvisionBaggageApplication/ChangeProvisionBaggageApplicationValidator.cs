using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBaggageApplication
{
    public abstract class ChangeProvisionBaggageApplicationValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionBaggageApplicationCommand
    {
        protected ChangeProvisionBaggageApplicationValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.BaggageApplication!)
                .SetValidator(new ProvisionBaggageApplicationInputValidator())
                .When(command => command.BaggageApplication is not null);
        }
    }
}
