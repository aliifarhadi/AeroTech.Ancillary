using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule
{
    public sealed class PriceRuleConditionsInputValidator : AbstractValidator<PriceRuleConditionsInput>
    {
        public PriceRuleConditionsInputValidator()
        {
            RuleForEach(conditions => conditions.PassengerTypes).IsInEnum();
        }
    }
}
