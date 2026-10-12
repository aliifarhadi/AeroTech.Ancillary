using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    [JsonDerivedType(typeof(WheelchairSpecificationDto))]
    [JsonDerivedType(typeof(DisabilityAssistanceSpecificationDto))]
    [JsonDerivedType(typeof(MedicalEquipmentSpecificationDto))]
    [JsonDerivedType(typeof(BassinetSpecificationDto))]
    [JsonDerivedType(typeof(UnaccompaniedMinorSpecificationDto))]
    public abstract record AssistedTravelVariantSpecificationDto;

    public sealed record WheelchairFormDto(
        [property: MinLength(1)] IReadOnlyList<string> AllowedSsrCodes,
        string? AssistanceLevelCode,
        int LeadTimeMinutes);

    public sealed record DisabilityAssistanceFormDto(
        [property: MinLength(1)] IReadOnlyList<string> AllowedSsrCodes,
        AssistanceCommunicationMethod? RequiredCommunicationMethod);

    public sealed record MedicalEquipmentFormDto(
        string MedicalServiceCode,
        bool RequiresMedicalApproval,
        MedicalEquipmentKind EquipmentKind,
        decimal? OxygenUnits,
        IReadOnlyList<string> EvidenceTypeCodes);

    public sealed record BassinetFormDto(
        decimal? MaxInfantWeightKg,
        int? MaxInfantAgeMonths,
        [property: MinLength(1)] IReadOnlyList<string> CompatibleSeatGroups);

    public sealed record UnaccompaniedMinorFormDto(
        int MinAgeYears,
        int MaxAgeYearsExclusive,
        MinorConnectionPolicy ConnectionPolicy,
        IReadOnlyList<int> AllowedTransitAirportIds);

    public sealed record WheelchairSpecificationDto(
        WheelchairFormDto Wheelchair) : AssistedTravelVariantSpecificationDto;

    public sealed record DisabilityAssistanceSpecificationDto(
        DisabilityAssistanceFormDto DisabilityAssistance) : AssistedTravelVariantSpecificationDto;

    public sealed record MedicalEquipmentSpecificationDto(
        MedicalEquipmentFormDto MedicalEquipment) : AssistedTravelVariantSpecificationDto;

    public sealed record BassinetSpecificationDto(
        BassinetFormDto Bassinet) : AssistedTravelVariantSpecificationDto;

    public sealed record UnaccompaniedMinorSpecificationDto(
        UnaccompaniedMinorFormDto UnaccompaniedMinor) : AssistedTravelVariantSpecificationDto;
}
