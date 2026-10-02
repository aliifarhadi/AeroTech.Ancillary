using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule
{
    public abstract class DefineAncillaryPriceRuleValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IDefineAncillaryPriceRuleCommand
    {
        protected DefineAncillaryPriceRuleValidator()
        {
            RuleFor(command => command.ProductRef).NotEmpty();
            RuleFor(command => command.Lines).NotNull();
            RuleForEach(command => command.Lines).NotNull().SetValidator(new PriceRuleLineValidator());
            RuleFor(command => command.Conditions).NotNull().SetValidator(new PriceRuleConditionsInputValidator());
        }
    }
}
