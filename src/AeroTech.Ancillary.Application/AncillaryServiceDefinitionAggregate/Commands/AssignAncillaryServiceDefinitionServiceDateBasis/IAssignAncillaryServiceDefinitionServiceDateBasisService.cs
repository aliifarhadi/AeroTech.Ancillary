using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis
{
    public interface IAssignAncillaryServiceDefinitionServiceDateBasisService
    {
        Task<ServiceDefinitionResult> AssignAsync(IAssignAncillaryServiceDefinitionServiceDateBasisCommand command, CancellationToken cancellationToken = default);
    }
}
