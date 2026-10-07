using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision
{
    public abstract class ActivateAncillaryProvisionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IActivateAncillaryProvisionCommand
    {
        protected ActivateAncillaryProvisionValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
        }
    }
}
