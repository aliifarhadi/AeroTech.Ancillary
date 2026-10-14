using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public static class AuthorizedPointsOfSale
    {
        public const string BlockedExternalReference = "BLOCKED_EXTERNAL_REFERENCE";

        public static async Task<IReadOnlyList<long>> ResolveAsync(
            IPointOfSaleEligibilityReference reference,
            PointOfSaleEligibilityQuery query,
            CancellationToken cancellationToken = default)
        {
            var eligibility = await reference.ResolveAsync(query, cancellationToken);

            return eligibility.Result == InventoryReferenceCheck.Verified
                ? eligibility.AllowedPointOfSaleIds.Distinct().Order().ToList()
                : throw ExceptionFactory.PointOfSaleContextUnresolved($"{BlockedExternalReference}: the point-of-sale reference is not connected and no point of sale was named");
        }
    }
}
