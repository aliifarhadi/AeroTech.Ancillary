using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate
{
    public abstract class ChangeProvisionTravelDateValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionTravelDateCommand
    {
        protected ChangeProvisionTravelDateValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.TravelDate!)
                .SetValidator(new ProvisionTravelDateInputValidator())
                .When(command => command.TravelDate is not null);
        }
    }
}
