using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.RetireAncillaryServiceDefinition.Backoffice
{
    public sealed record BackofficeRetireAncillaryServiceDefinitionCommand(long ServiceDefinitionId)
        : IRequest<ServiceDefinitionResult>, IRetireAncillaryServiceDefinitionCommand;
}
