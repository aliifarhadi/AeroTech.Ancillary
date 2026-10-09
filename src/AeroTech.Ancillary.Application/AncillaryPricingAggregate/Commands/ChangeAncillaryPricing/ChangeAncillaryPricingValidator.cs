using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing
{
    public abstract class ChangeAncillaryPricingValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeAncillaryPricingCommand
    {
        protected ChangeAncillaryPricingValidator()
        {
            RuleFor(command => command.PricingId).GreaterThan(0);
            RuleFor(command => command.Rates).NotNull();
            RuleForEach(command => command.Rates).NotNull().SetValidator(new PricingRateInputValidator());
        }
    }
}
