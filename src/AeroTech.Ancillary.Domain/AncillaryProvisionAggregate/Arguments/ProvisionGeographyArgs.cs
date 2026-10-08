namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionGeographyArgs(
        IReadOnlyList<int> OriginAirportIds,
        IReadOnlyList<int> DestinationAirportIds,
        IReadOnlyList<int> ViaAirportIds,
        IReadOnlyList<ProvisionRoutePairArgs> RoutePairs,
        IReadOnlyList<ProvisionServiceLocationArgs> ServiceLocations,
        IReadOnlyList<int> CoverageCountryIds)
    {
        public bool IsEmpty
            => OriginAirportIds.Count == 0
               && DestinationAirportIds.Count == 0
               && ViaAirportIds.Count == 0
               && RoutePairs.Count == 0
               && ServiceLocations.Count == 0
               && CoverageCountryIds.Count == 0;
    }
}
