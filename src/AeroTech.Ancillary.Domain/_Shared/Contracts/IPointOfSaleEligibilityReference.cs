using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public sealed record PointOfSaleEligibilityQuery(
        Channel Channel,
        long? CountryId,
        IReadOnlyList<long> AirlineOfficeIds,
        long? TravelAgencyOfficeId);

    public sealed record PointOfSaleEligibility(InventoryReferenceCheck Result, IReadOnlyCollection<long> AllowedPointOfSaleIds);

    public interface IPointOfSaleEligibilityReference
    {
        Task<PointOfSaleEligibility> ResolveAsync(PointOfSaleEligibilityQuery query, CancellationToken cancellationToken = default);
    }
}
