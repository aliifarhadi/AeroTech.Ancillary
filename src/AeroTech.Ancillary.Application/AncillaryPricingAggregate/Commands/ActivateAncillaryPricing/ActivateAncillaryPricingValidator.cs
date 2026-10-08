using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ActivateAncillaryPricing
{
    public abstract class ActivateAncillaryPricingValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IActivateAncillaryPricingCommand
    {
        protected ActivateAncillaryPricingValidator()
        {
            RuleFor(command => command.PricingId).GreaterThan(0);
        }
    }
}
