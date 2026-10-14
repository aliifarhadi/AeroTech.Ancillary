using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.ReferenceData.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.ServiceHost.ReferenceData
{
    public sealed class ReferenceDataAirlineReference : IAirlineReference
    {
        private readonly ReferenceDbContext _referenceData;

        public ReferenceDataAirlineReference(ReferenceDbContext referenceData) => _referenceData = referenceData;

        public async Task<IReadOnlyDictionary<int, string>> FindIataCodesAsync(IReadOnlyCollection<int> airlineIds, CancellationToken cancellationToken = default)
            => await _referenceData.Airlines.AsNoTracking()
                .Where(airline => airlineIds.Contains(airline.Id))
                .ToDictionaryAsync(airline => airline.Id, airline => airline.IataCode, cancellationToken);
    }
}
