using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    public sealed class MoneyInputValidator : AbstractValidator<MoneyInput>
    {
        public MoneyInputValidator()
        {
            RuleFor(money => money.CurrencyId).GreaterThan(0);
        }
    }

    public sealed class PricingRateInputValidator : AbstractValidator<PricingRateInput>
    {
        public PricingRateInputValidator()
        {
            RuleFor(rate => rate.PassengerTypeCode).IsInEnum().When(rate => rate.PassengerTypeCode is not null);
            RuleFor(rate => rate.BasePrice).NotNull().SetValidator(new MoneyInputValidator());
            RuleForEach(rate => rate.Components).NotNull().SetValidator(new PriceComponentInputValidator());
        }
    }

    public sealed class PriceComponentInputValidator : AbstractValidator<PriceComponentInput>
    {
        public PriceComponentInputValidator()
        {
            RuleFor(component => component.Category).IsInEnum();
            RuleFor(component => component.Code).NotEmpty().MaximumLength(10);
            RuleFor(component => component.Name).MaximumLength(100);
            RuleFor(component => component.Amount).NotNull().SetValidator(new MoneyInputValidator());
            RuleFor(component => component.FeeApplicationUnit).IsInEnum().When(component => component.FeeApplicationUnit is not null);
        }
    }
}
