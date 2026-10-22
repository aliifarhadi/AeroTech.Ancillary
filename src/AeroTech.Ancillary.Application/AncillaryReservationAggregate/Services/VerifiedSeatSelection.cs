namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public sealed record VerifiedSeatSelection(
        long OrderServiceId,
        long TravellerId,
        string SeatMapSelectionId,
        string SeatNumber,
        long FlightId,
        long ServiceDefinitionId,
        long ProvisionId,
        long PricingId);
}
