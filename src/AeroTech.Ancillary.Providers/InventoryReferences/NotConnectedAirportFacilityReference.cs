using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Providers.InventoryReferences
{
    public sealed class NotConnectedAirportFacilityReference : IAirportFacilityReference
    {
        public Task<AirportFacilityCheck> CheckAsync(int ownerAirlineId, long facilityId, CancellationToken cancellationToken = default)
            => Task.FromResult(new AirportFacilityCheck(InventoryReferenceCheck.SourceUnavailable, null, null));
    }
}
