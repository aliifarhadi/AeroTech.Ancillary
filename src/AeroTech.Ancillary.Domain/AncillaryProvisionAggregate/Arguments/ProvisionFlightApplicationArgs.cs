namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionFlightApplicationArgs(
        IReadOnlyList<int> MarketingAirlineIds,
        IReadOnlyList<int> OperatingAirlineIds,
        IReadOnlyList<string> FlightNumbers,
        IReadOnlyList<long> FlightIds,
        IReadOnlyList<int> AircraftIds)
    {
        public bool IsEmpty
            => MarketingAirlineIds.Count == 0
               && OperatingAirlineIds.Count == 0
               && FlightNumbers.Count == 0
               && FlightIds.Count == 0
               && AircraftIds.Count == 0;
    }
}
