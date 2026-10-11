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

        public async Task<IReadOnlyDictionary<int, int>> FindCountryIdsAsync(IReadOnlyCollection<int> airportIds, CancellationToken cancellationToken = default)
            => await _referenceData.Airports.AsNoTracking()
                .Where(airport => airportIds.Contains(airport.Id))
                .Join(
                    _referenceData.Cities.AsNoTracking().Where(city => city.CountryId != null),
                    airport => airport.CityId,
                    city => city.Id,
                    (airport, city) => new { AirportId = airport.Id, CountryId = city.CountryId!.Value })
                .ToDictionaryAsync(row => row.AirportId, row => row.CountryId, cancellationToken);
    }
}
