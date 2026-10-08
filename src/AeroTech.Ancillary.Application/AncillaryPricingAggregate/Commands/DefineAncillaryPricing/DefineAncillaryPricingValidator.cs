using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    public abstract class DefineAncillaryPricingValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IDefineAncillaryPricingCommand
    {
        protected DefineAncillaryPricingValidator()
        {
            RuleFor(command => command.AncillaryProvisionId).GreaterThan(0);
            RuleFor(command => command.FeeApplicationUnit).IsInEnum().When(command => command.FeeApplicationUnit is not null);
            RuleFor(command => command.PriceLines).NotNull();
            RuleForEach(command => command.PriceLines).NotNull().SetValidator(new PricingLineInputValidator());
        }
    }
}
