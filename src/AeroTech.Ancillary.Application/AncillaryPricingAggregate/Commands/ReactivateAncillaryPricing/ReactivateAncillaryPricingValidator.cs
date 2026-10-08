using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReactivateAncillaryPricing
{
    public abstract class ReactivateAncillaryPricingValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IReactivateAncillaryPricingCommand
    {
        protected ReactivateAncillaryPricingValidator()
        {
            RuleFor(command => command.PricingId).GreaterThan(0);
        }
    }
}
