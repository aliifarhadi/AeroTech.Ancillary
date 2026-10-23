using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficePetDetailsDto(
        EnumValueDto TransportMode,
        IReadOnlyList<BackofficePetAnimalDto> AllowedAnimalTypes,
        decimal MaxCombinedWeightKg,
        OfferDimensionsDto CarrierDimensionsMaxCm,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? MinAnimalAgeWeeks,
        IReadOnlyList<string> RequiredDocumentCodes,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<PetSizeBracketDto>? AllowedHoldAnimalSizeBrackets);
}
