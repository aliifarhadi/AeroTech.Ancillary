using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public sealed class LabSeatPrices : ISeatPriceVerifier
{
    public Task<IReadOnlyList<VerifiedSelection>> VerifyAsync(SeatPriceVerification verification, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<VerifiedOrderSelections> VerifyAsync(OrderSeatPriceVerification verification, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
