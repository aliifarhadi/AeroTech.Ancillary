using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition.Backoffice
{
    public sealed record BackofficeActivateAncillaryServiceDefinitionCommand(long ServiceDefinitionId)
        : IRequest<ServiceDefinitionResult>, IActivateAncillaryServiceDefinitionCommand;
}
