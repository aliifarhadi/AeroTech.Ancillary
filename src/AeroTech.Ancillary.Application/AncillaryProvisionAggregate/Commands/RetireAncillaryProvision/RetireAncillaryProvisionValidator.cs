using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision
{
    public abstract class RetireAncillaryProvisionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRetireAncillaryProvisionCommand
    {
        protected RetireAncillaryProvisionValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
        }
    }
}
