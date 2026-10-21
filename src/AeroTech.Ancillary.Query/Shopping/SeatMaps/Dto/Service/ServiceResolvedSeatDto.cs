namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceResolvedSeatDto(
        string Id,
        string TravellerRef,
        long FlightId,
        string SeatNumber,
        ServiceSeatDefinitionDto ServiceDefinition,
        ServiceSeatProvisionDto Provision,
        ServiceSeatPricingDto Pricing);
}
