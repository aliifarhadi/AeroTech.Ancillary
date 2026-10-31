using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public interface IPriceAncillaryOffersService
    {
        Task<SelectionSnapshotResult> SubmitAsync(IPriceAncillaryOffersCommand command, CancellationToken cancellationToken = default);

        Task<SelectionSnapshotResult> SubmitAsync(IPriceOrderAncillaryOffersCommand command, CancellationToken cancellationToken = default);

        Task<SelectionSnapshotResult> SubmitAsync(IPriceOfferSeatsCommand command, CancellationToken cancellationToken = default);

        Task<SelectionSnapshotResult> SubmitAsync(IPriceOrderSeatsCommand command, CancellationToken cancellationToken = default);
    }
}
