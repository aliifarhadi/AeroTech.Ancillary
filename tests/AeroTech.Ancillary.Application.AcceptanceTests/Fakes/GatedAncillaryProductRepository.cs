using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

public sealed class GatedAncillaryProductRepository(IAncillaryProductRepository inner) : IAncillaryProductRepository
{
    private readonly TaskCompletionSource _loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Loaded => _loaded.Task;

    public void Release() => _released.SetResult();

    public async Task<AncillaryProduct?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await inner.GetAsync(id, cancellationToken);

        _loaded.TrySetResult();
        await _released.Task;

        return product;
    }

    public Task AddAsync(AncillaryProduct product, CancellationToken cancellationToken = default)
        => inner.AddAsync(product, cancellationToken);

    public Task<bool> ExistsAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default)
        => inner.ExistsAsync(ownerAirlineId, productRef, cancellationToken);

    public Task<bool> DraftExistsAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default)
        => inner.DraftExistsAsync(ownerAirlineId, productRef, cancellationToken);

    public Task<int> HighestVersionAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default)
        => inner.HighestVersionAsync(ownerAirlineId, productRef, cancellationToken);

    public Task<AncillaryProduct?> FindOfferedVersionAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default)
        => inner.FindOfferedVersionAsync(ownerAirlineId, productRef, cancellationToken);

    public Task<IReadOnlyList<AncillaryProduct>> ListActiveAsync(CancellationToken cancellationToken = default)
        => inner.ListActiveAsync(cancellationToken);
}
