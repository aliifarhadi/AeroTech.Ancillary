using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public sealed record AncillarySalesScope(
        int OwnerAirlineId,
        Channel Channel,
        IReadOnlyList<long> PointOfSaleIds,
        long? CustomerId,
        int? PreferredCurrencyId,
        DateTimeOffset SalesInstantUtc);
}
