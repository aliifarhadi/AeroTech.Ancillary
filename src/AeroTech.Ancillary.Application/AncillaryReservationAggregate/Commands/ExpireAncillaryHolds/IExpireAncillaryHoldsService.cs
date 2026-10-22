namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ExpireAncillaryHolds
{
    public interface IExpireAncillaryHoldsService
    {
        Task<int> ExpireAsync(CancellationToken cancellationToken = default);
    }
}
