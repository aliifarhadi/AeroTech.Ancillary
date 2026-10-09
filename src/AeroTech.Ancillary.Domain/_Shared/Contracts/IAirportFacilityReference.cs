namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public sealed record AirportFacilityCheck(InventoryReferenceCheck Result, int? AirportId, string? TimeZoneId);

    public interface IAirportFacilityReference
    {
        Task<AirportFacilityCheck> CheckAsync(int ownerAirlineId, long facilityId, CancellationToken cancellationToken = default);
    }
}
