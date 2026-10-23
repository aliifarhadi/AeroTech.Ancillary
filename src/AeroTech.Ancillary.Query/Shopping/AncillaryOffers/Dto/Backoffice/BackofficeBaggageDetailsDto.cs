using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeBaggageDetailsDto(
        EnumValueDto ChargeKind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] EnumValueDto? AllowanceConcept,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? PackageWeightKg,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? MaxKgPerPiece,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? WeightFromExclusiveKg,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? WeightToInclusiveKg,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] OfferDimensionsDto? MaxSizeCm,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? MaxLinearSumCm,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? EquipmentKind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] EnumValueDto? ChargeCombination);
}
