using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReviseAncillaryPricing
{
    public abstract class ReviseAncillaryPricingValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IReviseAncillaryPricingCommand
    {
        protected ReviseAncillaryPricingValidator()
        {
            RuleFor(command => command.PricingId).GreaterThan(0);
        }
    }
}
