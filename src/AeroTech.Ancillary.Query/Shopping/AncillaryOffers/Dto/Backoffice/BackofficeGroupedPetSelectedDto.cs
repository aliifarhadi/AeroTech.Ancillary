using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedPetSelectedDto(
        EnumValueDto? AnimalType,
        string? OtherAnimalCode,
        int? AnimalAgeWeeks,
        decimal? CombinedWeightKg,
        BackofficeGroupedDimensionsDto? CarrierDimensions,
        IReadOnlyList<string>? DocumentAcknowledgements,
        string? SizeBracket) : BackofficeGroupedSelectedDto;
}
