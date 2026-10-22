namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedUpgradeSelectedDto(
        int? TargetCabinId,
        string? QuoteRef) : OtaPanelGroupedSelectedDto;
}
