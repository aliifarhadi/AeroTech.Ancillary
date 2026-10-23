using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record PetDetailsDto(
        PetTransportMode TransportMode,
        IReadOnlyList<PetAnimalDto> AllowedAnimalTypes,
        decimal MaxCombinedWeightKg,
        OfferDimensionsDto CarrierDimensionsMaxCm,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? MinAnimalAgeWeeks,
        IReadOnlyList<string> RequiredDocumentCodes,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<PetSizeBracketDto>? AllowedHoldAnimalSizeBrackets);
}
