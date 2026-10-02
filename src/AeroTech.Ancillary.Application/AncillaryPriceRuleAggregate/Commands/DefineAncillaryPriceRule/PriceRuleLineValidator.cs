using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule
{
    public sealed class PriceRuleLineValidator : AbstractValidator<PriceRuleLine>
    {
        public PriceRuleLineValidator()
        {
            RuleFor(line => line.Category).IsInEnum();
            RuleFor(line => line.Code).MaximumLength(10);
            RuleFor(line => line.Name).MaximumLength(100);
        }
    }
}
