using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis.Backoffice
{
    public sealed record BackofficeAssignAncillaryServiceDefinitionServiceDateBasisCommand(
        long ServiceDefinitionId,
        ServiceDateBasis ServiceDateBasis) : IRequest<ServiceDefinitionResult>, IAssignAncillaryServiceDefinitionServiceDateBasisCommand;
}
