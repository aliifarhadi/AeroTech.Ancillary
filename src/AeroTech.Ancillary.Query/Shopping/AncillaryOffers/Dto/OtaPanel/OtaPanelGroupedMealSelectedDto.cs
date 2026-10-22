namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedMealSelectedDto(
        string? MealCode,
        string? MenuItemRef,
        int? Quantity) : OtaPanelGroupedSelectedDto;
}
