using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public sealed record GroupedSelectionVerification(
        long OrderId,
        int OrderCommercialVersion,
        long PointOfSaleId,
        int CurrencyId,
        DateTimeOffset? TicketedAtUtc,
        IReadOnlyList<GroupedHoldSelectionEntry> Selections);
}
