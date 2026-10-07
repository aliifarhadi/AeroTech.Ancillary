using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition
{
    public abstract class ActivateAncillaryServiceDefinitionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IActivateAncillaryServiceDefinitionCommand
    {
        protected ActivateAncillaryServiceDefinitionValidator()
        {
            RuleFor(command => command.ServiceDefinitionId).GreaterThan(0);
        }
    }
}
