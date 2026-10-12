using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.ReferenceData.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.ServiceHost.ReferenceData
{
    public sealed class ReferenceDataAirFareReference : IAirFareReference
    {
        private readonly ReferenceDbContext _referenceData;

        public ReferenceDataAirFareReference(ReferenceDbContext referenceData) => _referenceData = referenceData;

        public async Task<IReadOnlyDictionary<long, long>> FindFareFamilyIdsAsync(IReadOnlyCollection<long> airFareIds, CancellationToken cancellationToken = default)
            => await _referenceData.AirFares.AsNoTracking()
                .Where(fare => airFareIds.Contains(fare.Id) && fare.FareFamilyId != null)
                .ToDictionaryAsync(fare => fare.Id, fare => fare.FareFamilyId!.Value, cancellationToken);
    }
}
