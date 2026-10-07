namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public interface IHoldAncillaryServicesService
    {
        Task<AncillaryHoldResult> HoldAsync(IHoldAncillaryServicesCommand command, CancellationToken cancellationToken = default);
    }
}
