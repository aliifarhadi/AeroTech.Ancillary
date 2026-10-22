using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedMealSelectedDto(
        string? MealCode,
        string? MenuItemRef,
        int? Quantity) : BackofficeGroupedSelectedDto;
}
