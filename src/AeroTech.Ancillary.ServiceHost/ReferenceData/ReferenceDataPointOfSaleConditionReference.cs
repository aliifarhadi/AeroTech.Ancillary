using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.ReferenceData.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.ServiceHost.ReferenceData
{
    public sealed class ReferenceDataPointOfSaleConditionReference : IPointOfSaleConditionReference
    {
        private readonly ReferenceDbContext _referenceData;

        public ReferenceDataPointOfSaleConditionReference(ReferenceDbContext referenceData) => _referenceData = referenceData;

        public async Task<PointOfSaleConditionSet> GetConditionSetAsync(CancellationToken cancellationToken = default)
        {
            var pointOfSaleIds = await _referenceData.PointOfSales.AsNoTracking()
                .Select(pointOfSale => pointOfSale.Id)
                .ToListAsync(cancellationToken);
            var conditions = await _referenceData.PointOfSaleConditions.AsNoTracking().ToListAsync(cancellationToken);

            return new PointOfSaleConditionSet(
                pointOfSaleIds,
                conditions
                    .Select(condition => new PointOfSaleCondition
                    {
                        PointOfSaleId = condition.PointOfSaleId,
                        CountryIsInclude = condition.CountryIsInclude,
                        CountryIds = condition.CountryIds.Select(countryId => (long)countryId).ToList(),
                        AirlineOfficeIsInclude = condition.AirlineOfficeIsInclude,
                        AirlineOfficeIds = condition.AirlineOfficeIds,
                        TravelAgencyOfficeIsInclude = condition.TravelAgencyOfficeIsInclude,
                        TravelAgencyOfficeIds = condition.TravelAgencyOfficeIds,
                        ChannelIsInclude = condition.ChannelIsInclude,
                        Channels = condition.Channels
                    })
                    .ToList());
        }
    }
}
