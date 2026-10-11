using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Snapshots
{
    public sealed record AncillarySalesScope(
        int OwnerAirlineId,
        Channel Channel,
        IReadOnlyList<long> PointOfSaleIds,
        long? CustomerId,
        int? PreferredCurrencyId,
        DateTimeOffset SalesInstantUtc);
}
