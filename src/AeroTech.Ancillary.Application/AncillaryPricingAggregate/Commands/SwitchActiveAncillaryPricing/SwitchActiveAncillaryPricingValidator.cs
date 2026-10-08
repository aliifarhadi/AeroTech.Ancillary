using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing
{
    public abstract class SwitchActiveAncillaryPricingValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ISwitchActiveAncillaryPricingCommand
    {
        protected SwitchActiveAncillaryPricingValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.NewPricingId).GreaterThan(0);
            RuleFor(command => command.ExpectedOldPricingId).GreaterThan(0).When(command => command.ExpectedOldPricingId is not null);
        }
    }
}
