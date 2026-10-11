using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Providers.PointOfSale
{
    public sealed class NotConnectedPointOfSaleEligibilityReference : IPointOfSaleEligibilityReference
    {
        public Task<PointOfSaleEligibility> ResolveAsync(PointOfSaleEligibilityQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(new PointOfSaleEligibility(InventoryReferenceCheck.SourceUnavailable, []));
    }
}
