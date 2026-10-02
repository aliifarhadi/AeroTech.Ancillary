using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.SuspendAncillaryPriceRule
{
    public abstract class SuspendAncillaryPriceRuleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ISuspendAncillaryPriceRuleCommand
    {
        protected SuspendAncillaryPriceRuleValidator()
        {
            RuleFor(command => command.AncillaryPriceRuleId).GreaterThan(0);
        }
    }
}
