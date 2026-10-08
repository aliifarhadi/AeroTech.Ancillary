using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit
{
    public abstract class AssignAncillaryServiceDefinitionPricingUnitValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IAssignAncillaryServiceDefinitionPricingUnitCommand
    {
        protected AssignAncillaryServiceDefinitionPricingUnitValidator()
        {
            RuleFor(command => command.ServiceDefinitionId).GreaterThan(0);
            RuleFor(command => command.PricingUnit).IsInEnum();
        }
    }
}
