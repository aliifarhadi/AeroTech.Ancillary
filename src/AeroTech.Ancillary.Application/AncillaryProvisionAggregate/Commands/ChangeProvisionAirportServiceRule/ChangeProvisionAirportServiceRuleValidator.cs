using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAirportServiceRule
{
    public abstract class ChangeProvisionAirportServiceRuleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionAirportServiceRuleCommand
    {
        protected ChangeProvisionAirportServiceRuleValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.AirportServiceRule!)
                .SetValidator(new ProvisionAirportServiceRuleInputValidator())
                .When(command => command.AirportServiceRule is not null);
        }
    }
}
