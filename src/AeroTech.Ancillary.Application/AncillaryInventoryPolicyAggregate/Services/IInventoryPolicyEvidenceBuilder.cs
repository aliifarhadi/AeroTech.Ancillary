using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Services
{
    public interface IInventoryPolicyEvidenceBuilder
    {
        Task<InventoryPolicyEvidence> BuildAsync(AncillaryInventoryPolicy policy, CancellationToken cancellationToken = default);
    }
}
