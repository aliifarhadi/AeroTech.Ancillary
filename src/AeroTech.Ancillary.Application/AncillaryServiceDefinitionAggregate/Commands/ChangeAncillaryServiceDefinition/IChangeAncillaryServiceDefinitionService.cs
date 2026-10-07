using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition
{
    public interface IChangeAncillaryServiceDefinitionService
    {
        Task<ServiceDefinitionResult> ChangeAsync(IChangeAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken = default);
    }
}
