using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionSalesRestrictionsArgs(
        DateTimeOffset? SalesEffectiveFrom,
        DateTimeOffset? SalesDiscontinueAt,
        IReadOnlyList<long> PointOfSaleIds,
        IReadOnlyList<long> CustomerIds,
        IReadOnlyList<CustomerType> CustomerTypes)
    {
        public bool IsEmpty
            => SalesEffectiveFrom is null
               && SalesDiscontinueAt is null
               && PointOfSaleIds.Count == 0
               && CustomerIds.Count == 0
               && CustomerTypes.Count == 0;
    }
}
