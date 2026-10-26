namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record HeldConnectivityOptionsDto(HeldConnectivitySelectionDto Connectivity);

    public sealed record HeldConnectivitySelectionDto(int DeviceCount);
}
