using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition.Backoffice
{
    public sealed record BackofficeReactivateAncillaryServiceDefinitionCommand(long ServiceDefinitionId)
        : IRequest<ServiceDefinitionResult>, IReactivateAncillaryServiceDefinitionCommand;
}
