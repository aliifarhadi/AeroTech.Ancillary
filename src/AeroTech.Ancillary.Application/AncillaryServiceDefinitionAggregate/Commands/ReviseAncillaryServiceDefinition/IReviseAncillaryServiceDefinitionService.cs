using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReviseAncillaryServiceDefinition
{
    public interface IReviseAncillaryServiceDefinitionService
    {
        Task<ServiceDefinitionResult> ReviseAsync(IReviseAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken = default);
    }
}
