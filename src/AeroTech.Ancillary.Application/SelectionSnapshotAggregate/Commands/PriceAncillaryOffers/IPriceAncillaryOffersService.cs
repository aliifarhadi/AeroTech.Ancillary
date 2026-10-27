namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public interface IPriceAncillaryOffersService
    {
        Task<SelectionSnapshotResult> SubmitAsync(IPriceAncillaryOffersCommand command, CancellationToken cancellationToken = default);

        Task<SelectionSnapshotResult> SubmitAsync(IPriceOrderAncillaryOffersCommand command, CancellationToken cancellationToken = default);
    }
}
