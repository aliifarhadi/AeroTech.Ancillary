using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    [JsonDerivedType(typeof(StandardSeatSpecificationDto))]
    [JsonDerivedType(typeof(PreferredSeatSpecificationDto))]
    [JsonDerivedType(typeof(ExtraSeatSpecificationDto))]
    public abstract record SeatVariantSpecificationDto;

    [VariantCode(AncillaryVariant.StandardSeat)]
    public sealed record StandardSeatSpecificationDto(
        IReadOnlyList<string> SeatCharacteristicCodes,
        IReadOnlyList<int> ApplicableCabinIds,
        bool RequiresExitRowEligibility) : SeatVariantSpecificationDto;

    [VariantCode(AncillaryVariant.PreferredSeat)]
    public sealed record PreferredSeatSpecificationDto(
        [property: MinLength(1)] IReadOnlyList<string> SeatCharacteristicCodes,
        IReadOnlyList<int> ApplicableCabinIds,
        bool RequiresExitRowEligibility) : SeatVariantSpecificationDto;

    [VariantCode(AncillaryVariant.ExtraSeat)]
    public sealed record ExtraSeatSpecificationDto(
        IReadOnlyList<string> SeatCharacteristicCodes,
        IReadOnlyList<int> ApplicableCabinIds,
        bool RequiresExitRowEligibility,
        bool RequiresAdjacentSeat,
        ExtraSeatPurpose ExtraSeatPurpose,
        [property: Range(1, int.MaxValue)] int ExtraOccupiedSeatCount,
        bool RequiresExternalTicketAction) : SeatVariantSpecificationDto;
}
