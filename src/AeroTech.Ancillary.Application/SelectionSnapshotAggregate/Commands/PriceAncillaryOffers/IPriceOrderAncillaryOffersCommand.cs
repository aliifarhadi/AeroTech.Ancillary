namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public interface IPriceOrderAncillaryOffersCommand : IPriceSelectionsCommand
    {
        long? OrderId { get; }
    }
}
