using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public sealed record HoldSelection(
        AncillaryProfile Family,
        string VariantCode,
        long ProvisionId,
        long OrderServiceId,
        long? TravellerId,
        IReadOnlyList<long> FlightIds,
        int? Quantity,
        ISelectionOptions? Options);
}
