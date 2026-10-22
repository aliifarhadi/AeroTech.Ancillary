using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedBaggageSelectedDto(
        int? Quantity,
        string? PackageProductRef,
        string? BagRef,
        decimal? MeasuredWeightKg,
        BackofficeGroupedDimensionsDto? Dimensions,
        string? EquipmentKind,
        decimal? WeightKg) : BackofficeGroupedSelectedDto;
}
