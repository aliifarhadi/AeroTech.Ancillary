using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    public sealed class PricingLineInputValidator : AbstractValidator<PricingLineInput>
    {
        public PricingLineInputValidator()
        {
            RuleFor(line => line.PassengerTypeCode).IsInEnum().When(line => line.PassengerTypeCode is not null);
            RuleFor(line => line.Category).IsInEnum();
            RuleFor(line => line.Code).MaximumLength(10);
            RuleFor(line => line.Name).MaximumLength(100);
        }
    }
}
