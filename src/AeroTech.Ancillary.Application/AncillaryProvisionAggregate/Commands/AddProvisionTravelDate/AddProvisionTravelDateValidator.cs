using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate
{
    public abstract class AddProvisionTravelDateValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IAddProvisionTravelDateCommand
    {
        protected AddProvisionTravelDateValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
        }
    }
}
