namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedBaggageSelectedDto(
        int? Quantity,
        string? PackageProductRef,
        string? BagRef,
        decimal? MeasuredWeightKg,
        OtaPanelGroupedDimensionsDto? Dimensions,
        string? EquipmentKind,
        decimal? WeightKg) : OtaPanelGroupedSelectedDto;
}
