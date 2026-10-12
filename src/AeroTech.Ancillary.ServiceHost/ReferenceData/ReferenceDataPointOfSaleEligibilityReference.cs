using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.ReferenceData.PointOfSales;
using AeroTech.Messages.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.ServiceHost.ReferenceData
{
    public sealed class ReferenceDataPointOfSaleEligibilityReference : IPointOfSaleEligibilityReference
    {
        private readonly ReferenceDbContext _referenceData;

        public ReferenceDataPointOfSaleEligibilityReference(ReferenceDbContext referenceData) => _referenceData = referenceData;

        public async Task<PointOfSaleEligibility> ResolveAsync(PointOfSaleEligibilityQuery query, CancellationToken cancellationToken = default)
        {
            var pointOfSaleIds = await _referenceData.PointOfSales.AsNoTracking()
                .Select(pointOfSale => pointOfSale.Id)
                .ToListAsync(cancellationToken);
            var conditions = await _referenceData.PointOfSaleConditions.AsNoTracking().ToListAsync(cancellationToken);

            return new PointOfSaleEligibility(
                InventoryReferenceCheck.Verified,
                PointOfSaleEligibilityResolver.ResolveAllowedPointOfSaleIds(
                    pointOfSaleIds,
                    conditions,
                    query.CountryId,
                    query.AirlineOfficeIds,
                    query.TravelAgencyOfficeId,
                    AirPriceChannel(query.Channel)));
        }

        private static string AirPriceChannel(Channel channel) => channel switch
        {
            Channel.PartnerAPI => "API",
            Channel.AgencyPanel => "GUI",
            _ => channel.ToString()
        };
    }
}
