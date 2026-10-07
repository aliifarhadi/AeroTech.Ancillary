using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReviseAncillaryServiceDefinition.Backoffice
{
    public sealed record BackofficeReviseAncillaryServiceDefinitionCommand(long ServiceDefinitionId)
        : IRequest<ServiceDefinitionResult>, IReviseAncillaryServiceDefinitionCommand;
}
