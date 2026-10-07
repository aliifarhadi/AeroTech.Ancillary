using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition
{
    public interface IReactivateAncillaryServiceDefinitionService
    {
        Task<ServiceDefinitionResult> ReactivateAsync(IReactivateAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken = default);
    }
}
