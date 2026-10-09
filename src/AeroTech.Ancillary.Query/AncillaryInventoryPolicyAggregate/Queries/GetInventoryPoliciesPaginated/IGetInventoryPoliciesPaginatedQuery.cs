using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPoliciesPaginated
{
    public interface IGetInventoryPoliciesPaginatedQuery
    {
        string? ServiceDefinitionRef { get; }

        InventoryAuthority? Authority { get; }

        LocalInventoryPattern? LocalPattern { get; }

        InventoryRecordStatus? Status { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
