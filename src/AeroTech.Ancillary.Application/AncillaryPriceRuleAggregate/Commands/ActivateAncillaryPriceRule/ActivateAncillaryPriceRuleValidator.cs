using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ActivateAncillaryPriceRule
{
    public abstract class ActivateAncillaryPriceRuleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IActivateAncillaryPriceRuleCommand
    {
        protected ActivateAncillaryPriceRuleValidator()
        {
            RuleFor(command => command.AncillaryPriceRuleId).GreaterThan(0);
        }
    }
}
