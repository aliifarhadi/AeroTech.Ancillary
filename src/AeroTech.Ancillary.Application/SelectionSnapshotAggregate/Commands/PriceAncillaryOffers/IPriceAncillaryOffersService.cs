namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public interface IPriceAncillaryOffersService
    {
        Task<SelectionSnapshotResult> SubmitAsync(IPriceAncillaryOffersCommand command, CancellationToken cancellationToken = default);
    }
}
