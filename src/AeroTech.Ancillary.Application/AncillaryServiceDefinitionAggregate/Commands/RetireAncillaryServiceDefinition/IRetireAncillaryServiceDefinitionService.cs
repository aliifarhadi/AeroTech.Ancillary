using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.RetireAncillaryServiceDefinition
{
    public interface IRetireAncillaryServiceDefinitionService
    {
        Task<ServiceDefinitionResult> RetireAsync(IRetireAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken = default);
    }
}
