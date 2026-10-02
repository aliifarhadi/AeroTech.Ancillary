using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ChangeAncillaryPriceRule
{
    public abstract class ChangeAncillaryPriceRuleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeAncillaryPriceRuleCommand
    {
        protected ChangeAncillaryPriceRuleValidator()
        {
            RuleFor(command => command.AncillaryPriceRuleId).GreaterThan(0);
            RuleFor(command => command.Lines).NotNull();
            RuleForEach(command => command.Lines).NotNull().SetValidator(new PriceRuleLineValidator());
            RuleFor(command => command.Conditions).NotNull().SetValidator(new PriceRuleConditionsInputValidator());
        }
    }
}
