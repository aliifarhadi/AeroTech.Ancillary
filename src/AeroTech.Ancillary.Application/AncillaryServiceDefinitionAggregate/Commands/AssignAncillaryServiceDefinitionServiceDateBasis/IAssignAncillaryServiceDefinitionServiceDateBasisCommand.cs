using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis
{
    public interface IAssignAncillaryServiceDefinitionServiceDateBasisCommand
    {
        long ServiceDefinitionId { get; }

        ServiceDateBasis ServiceDateBasis { get; }
    }
}
