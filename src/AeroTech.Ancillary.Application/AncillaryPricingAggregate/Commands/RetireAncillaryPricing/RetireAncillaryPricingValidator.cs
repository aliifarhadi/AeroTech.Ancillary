using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.RetireAncillaryPricing
{
    public abstract class RetireAncillaryPricingValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRetireAncillaryPricingCommand
    {
        protected RetireAncillaryPricingValidator()
        {
            RuleFor(command => command.PricingId).GreaterThan(0);
        }
    }
}
