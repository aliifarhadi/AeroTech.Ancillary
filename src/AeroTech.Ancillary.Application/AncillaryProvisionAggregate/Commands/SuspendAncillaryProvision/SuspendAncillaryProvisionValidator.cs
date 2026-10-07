using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision
{
    public abstract class SuspendAncillaryProvisionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ISuspendAncillaryProvisionCommand
    {
        protected SuspendAncillaryProvisionValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
        }
    }
}
