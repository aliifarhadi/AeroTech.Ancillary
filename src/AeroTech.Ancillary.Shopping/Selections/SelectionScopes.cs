using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public static class SelectionScopes
    {
        public static SelectionScope Of(ServiceCoverageScope scope)
            => scope switch
            {
                ServiceCoverageScope.Order => SelectionScope.PerOrder,
                ServiceCoverageScope.Journey => SelectionScope.PerTraveller,
                ServiceCoverageScope.Portion => SelectionScope.PerTravellerPerPortion,
                ServiceCoverageScope.Sector => SelectionScope.PerTravellerPerFlight,
                _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
            };
    }
}
