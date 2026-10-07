using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.SuspendAncillaryServiceDefinition.Backoffice
{
    public sealed record BackofficeSuspendAncillaryServiceDefinitionCommand(long ServiceDefinitionId)
        : IRequest<ServiceDefinitionResult>, ISuspendAncillaryServiceDefinitionCommand;
}
