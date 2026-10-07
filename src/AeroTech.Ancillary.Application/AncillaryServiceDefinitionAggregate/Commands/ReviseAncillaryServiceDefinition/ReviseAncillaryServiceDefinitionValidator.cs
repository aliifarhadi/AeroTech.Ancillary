using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReviseAncillaryServiceDefinition
{
    public abstract class ReviseAncillaryServiceDefinitionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IReviseAncillaryServiceDefinitionCommand
    {
        protected ReviseAncillaryServiceDefinitionValidator()
        {
            RuleFor(command => command.ServiceDefinitionId).GreaterThan(0);
        }
    }
}
