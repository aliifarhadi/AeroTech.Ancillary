namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedExtraSeatSelectedDto(
        string? SeatNumber,
        string? Purpose) : OtaPanelGroupedSelectedDto;
}
