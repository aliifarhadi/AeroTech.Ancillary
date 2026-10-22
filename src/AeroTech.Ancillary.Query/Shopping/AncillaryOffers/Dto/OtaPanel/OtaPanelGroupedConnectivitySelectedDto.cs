namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedConnectivitySelectedDto(
        string? PlanCode,
        int? DeviceCount) : OtaPanelGroupedSelectedDto;
}
