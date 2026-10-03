using AeroTech.Ancillary.Domain.ServiceReservationAggregate;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Contracts;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

public sealed class GatedServiceReservationRepository(IServiceReservationRepository inner) : IServiceReservationRepository
{
    private readonly TaskCompletionSource _loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Loaded => _loaded.Task;

    public void Release() => _released.SetResult();

    public async Task<ServiceReservation?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var reservation = await inner.GetAsync(id, cancellationToken);

        _loaded.TrySetResult();
        await _released.Task;

        return reservation;
    }

    public async Task<ServiceReservation?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var reservation = await inner.FindByIdempotencyKeyAsync(idempotencyKey, cancellationToken);

        _loaded.TrySetResult();
        await _released.Task;

        return reservation;
    }

    public Task AddAsync(ServiceReservation reservation, CancellationToken cancellationToken = default)
        => inner.AddAsync(reservation, cancellationToken);

    public void Update(ServiceReservation reservation) => inner.Update(reservation);
}
