using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    public abstract class DefineAncillaryPricingValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IDefineAncillaryPricingCommand
    {
        protected DefineAncillaryPricingValidator()
        {
            RuleFor(command => command.AncillaryProvisionId).GreaterThan(0);
            RuleFor(command => command.Rates).NotNull();
            RuleForEach(command => command.Rates).NotNull().SetValidator(new PricingRateInputValidator());
        }
    }
}
