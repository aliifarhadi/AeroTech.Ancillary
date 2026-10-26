namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments
{
    public sealed record AncillaryReservationServiceSelectionArgs(
        long OrderServiceId,
        long ProvisionId,
        long? TravellerId,
        IReadOnlyList<long> CoveredFlightIds,
        int? Quantity,
        AncillaryReservationSelectionArgs? Options);
}
