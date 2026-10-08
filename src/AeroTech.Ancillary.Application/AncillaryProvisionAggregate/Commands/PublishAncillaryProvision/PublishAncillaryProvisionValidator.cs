using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision
{
    public abstract class PublishAncillaryProvisionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IPublishAncillaryProvisionCommand
    {
        protected PublishAncillaryProvisionValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.PricingId).GreaterThan(0);
        }
    }
}
