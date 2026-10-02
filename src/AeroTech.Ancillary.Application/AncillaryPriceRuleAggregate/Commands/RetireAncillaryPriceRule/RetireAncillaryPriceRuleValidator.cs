using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.RetireAncillaryPriceRule
{
    public abstract class RetireAncillaryPriceRuleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRetireAncillaryPriceRuleCommand
    {
        protected RetireAncillaryPriceRuleValidator()
        {
            RuleFor(command => command.AncillaryPriceRuleId).GreaterThan(0);
        }
    }
}
