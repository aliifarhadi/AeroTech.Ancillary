using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record BaggageDetailsDto(
        BaggageChargeKind ChargeKind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] BaggageAllowanceConcept? AllowanceConcept,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? PackageWeightKg,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? MaxKgPerPiece,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? WeightFromExclusiveKg,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? WeightToInclusiveKg,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] OfferDimensionsDto? MaxSizeCm,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? MaxLinearSumCm,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? EquipmentKind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] BaggageChargeCombination? ChargeCombination);
}
