using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.SeatMaps;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

public sealed class TestOnlySeatSelectionVerifier : ISeatSelectionVerifier
{
    public Dictionary<long, (long DefinitionId, long ProvisionId)> Catalog { get; } = [];

    public List<SeatSelectionVerification> Verified { get; } = [];

    public Task<IReadOnlyList<VerifiedSeatSelection>> VerifyAsync(SeatSelectionVerification verification, CancellationToken cancellationToken = default)
    {
        Verified.Add(verification);

        return Task.FromResult<IReadOnlyList<VerifiedSeatSelection>>(verification.Seats
            .Select(seat =>
            {
                var decoded = SeatMapSelectionId.Decode(seat.SeatMapSelectionId) ?? throw ExceptionFactory.AncillaryHoldSeatNotSellable(seat.SeatMapSelectionId, "unreadable");

                if (!Catalog.TryGetValue(decoded.PricingId, out var product))
                    throw ExceptionFactory.AncillaryHoldSeatNotSellable(seat.SeatMapSelectionId, "SeatSelectionNotOffered");

                return new VerifiedSeatSelection(seat.OrderServiceId, seat.TravellerId, seat.SeatMapSelectionId, decoded.SeatNumber, decoded.FlightId, product.DefinitionId, product.ProvisionId, decoded.PricingId);
            })
            .ToList());
    }
}
