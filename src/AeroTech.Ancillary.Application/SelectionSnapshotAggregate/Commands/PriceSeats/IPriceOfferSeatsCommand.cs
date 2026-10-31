namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats
{
    public interface IPriceOfferSeatsCommand : IPriceSeatsCommand
    {
        string? OfferId { get; }
    }
}
