using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SuspendAncillaryPricing
{
    public abstract class SuspendAncillaryPricingValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ISuspendAncillaryPricingCommand
    {
        protected SuspendAncillaryPricingValidator()
        {
            RuleFor(command => command.PricingId).GreaterThan(0);
        }
    }
}
