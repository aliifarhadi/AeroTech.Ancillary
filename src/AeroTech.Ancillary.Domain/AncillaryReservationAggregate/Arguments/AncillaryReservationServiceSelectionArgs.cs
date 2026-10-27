using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments
{
    public sealed record AncillaryReservationServiceSelectionArgs(
        AncillaryProfile Family,
        string VariantCode,
        long OrderServiceId,
        long ProvisionId,
        long? TravellerId,
        IReadOnlyList<long> CoveredFlightIds,
        int? Quantity,
        AncillaryReservationSelectionArgs? Options);
}
