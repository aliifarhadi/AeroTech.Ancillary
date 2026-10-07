using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.SuspendAncillaryServiceDefinition
{
    public interface ISuspendAncillaryServiceDefinitionService
    {
        Task<ServiceDefinitionResult> SuspendAsync(ISuspendAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken = default);
    }
}
