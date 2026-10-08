using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis
{
    public abstract class AssignAncillaryServiceDefinitionServiceDateBasisValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IAssignAncillaryServiceDefinitionServiceDateBasisCommand
    {
        protected AssignAncillaryServiceDefinitionServiceDateBasisValidator()
        {
            RuleFor(command => command.ServiceDefinitionId).GreaterThan(0);
            RuleFor(command => command.ServiceDateBasis).IsInEnum();
        }
    }
}
