using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.SuspendAncillaryServiceDefinition
{
    public abstract class SuspendAncillaryServiceDefinitionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ISuspendAncillaryServiceDefinitionCommand
    {
        protected SuspendAncillaryServiceDefinitionValidator()
        {
            RuleFor(command => command.ServiceDefinitionId).GreaterThan(0);
        }
    }
}
