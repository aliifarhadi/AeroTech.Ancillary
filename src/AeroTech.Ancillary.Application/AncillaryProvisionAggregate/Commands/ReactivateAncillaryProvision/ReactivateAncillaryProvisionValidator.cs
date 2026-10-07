using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision
{
    public abstract class ReactivateAncillaryProvisionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IReactivateAncillaryProvisionCommand
    {
        protected ReactivateAncillaryProvisionValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
        }
    }
}
