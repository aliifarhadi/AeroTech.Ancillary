using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFlightApplication
{
    public abstract class ChangeProvisionFlightApplicationValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionFlightApplicationCommand
    {
        protected ChangeProvisionFlightApplicationValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.FlightApplication!)
                .SetValidator(new ProvisionFlightApplicationInputValidator())
                .When(command => command.FlightApplication is not null);
        }
    }
}
