using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.SelectionIds
{
    public sealed record NonSeatSelectionScope(
        Channel Channel,
        long PointOfSaleId,
        long? AirlineOfficeId,
        long? TravelAgencyId,
        long? CustomerId,
        long? CountryId)
    {
        public static NonSeatSelectionScope Of(AncillarySalesScope scope, long pointOfSaleId)
            => new(scope.Channel, pointOfSaleId, scope.AirlineOfficeId, scope.TravelAgencyId, scope.CustomerId, scope.CountryId);

        public AncillarySalesScope ToSalesScope(int ownerAirlineId)
            => new(ownerAirlineId, Channel, [PointOfSaleId], TravelAgencyId, AirlineOfficeId, CustomerId, CountryId);
    }
}
