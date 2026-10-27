namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public interface IPriceAncillaryOffersCommand : IPriceSelectionsCommand
    {
        string? OfferId { get; }
    }
}
