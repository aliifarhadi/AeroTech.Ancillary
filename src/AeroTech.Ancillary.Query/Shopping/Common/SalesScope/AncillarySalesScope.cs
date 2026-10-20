using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public sealed record AncillarySalesScope(
        int OwnerAirlineId,
        Channel Channel,
        IReadOnlyList<long> PointOfSaleIds,
        long? TravelAgencyId = null,
        long? AirlineOfficeId = null,
        long? CustomerId = null,
        long? CountryId = null)
    {
        public bool SellsForAnyCustomer => AirlineOfficeId is not null && CustomerId is null;
    }
}
