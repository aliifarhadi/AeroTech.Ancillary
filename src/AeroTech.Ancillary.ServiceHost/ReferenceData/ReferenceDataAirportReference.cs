using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.ReferenceData.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.ServiceHost.ReferenceData
{
    public sealed class ReferenceDataAirportReference : IAirportReference
    {
        private readonly ReferenceDbContext _referenceData;

        public ReferenceDataAirportReference(ReferenceDbContext referenceData) => _referenceData = referenceData;

        public Task<bool> ExistsAsync(int airportId, CancellationToken cancellationToken = default)
            => _referenceData.Airports.AsNoTracking().AnyAsync(airport => airport.Id == airportId, cancellationToken);
    }
}
