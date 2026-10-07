using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition
{
    public interface IActivateAncillaryServiceDefinitionService
    {
        Task<ServiceDefinitionResult> ActivateAsync(IActivateAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken = default);
    }
}
