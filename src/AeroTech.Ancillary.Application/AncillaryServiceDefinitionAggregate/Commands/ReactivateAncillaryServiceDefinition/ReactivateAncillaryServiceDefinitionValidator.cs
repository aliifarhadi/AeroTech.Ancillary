using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition
{
    public abstract class ReactivateAncillaryServiceDefinitionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IReactivateAncillaryServiceDefinitionCommand
    {
        protected ReactivateAncillaryServiceDefinitionValidator()
        {
            RuleFor(command => command.ServiceDefinitionId).GreaterThan(0);
        }
    }
}
