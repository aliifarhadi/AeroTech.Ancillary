using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.RestApi.V1._Shared.OpenApi;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record BassinetDetailsRequest(
        decimal? MaxInfantWeightKg,
        int? MaxInfantAgeMonths,
        IReadOnlyList<string>? CompatibleSeatGroups,
        bool? RequiresInfantAndGuardian)
    {
        public BassinetDetailsInput ToInput()
            => new(
                MaxInfantWeightKg,
                MaxInfantAgeMonths,
                CompatibleSeatGroups,
                RequiresInfantAndGuardian ?? true);
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record UnaccompaniedMinorDetailsRequest(
        int MinAgeYears,
        int MaxAgeYearsExclusive,
        bool? GuardianContactRequired,
        MinorConnectionPolicy ConnectionPolicy,
        IReadOnlyList<int>? AllowedTransitAirportIds)
    {
        public UnaccompaniedMinorDetailsInput ToInput()
            => new(
                MinAgeYears,
                MaxAgeYearsExclusive,
                GuardianContactRequired ?? true,
                ConnectionPolicy,
                AllowedTransitAirportIds);
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    [VariantSpecification(
        typeof(AssistedTravelVariantSpecificationDto),
        "The assistance details of the variant named by variantCode: A15 Wheelchair (wheelchair), A16 Disability assistance " +
        "(disabilityAssistance), A17 Medical equipment (medicalEquipment), A18 Bassinet (bassinet), A19 Unaccompanied minor " +
        "(unaccompaniedMinor). The first v1 body is still accepted: assistanceKind, bassinet.requiresInfantAndGuardian and " +
        "unaccompaniedMinor.guardianContactRequired may be sent and must then agree with the variant, otherwise the answer is 422 with code 16202.")]
    public sealed record AssistedTravelSpecificationRequest(
        AssistanceKind? AssistanceKind,
        WheelchairDetailsInput? Wheelchair = null,
        DisabilityAssistanceDetailsInput? DisabilityAssistance = null,
        MedicalEquipmentDetailsInput? MedicalEquipment = null,
        BassinetDetailsRequest? Bassinet = null,
        UnaccompaniedMinorDetailsRequest? UnaccompaniedMinor = null)
    {
        public AssistedTravelSpecificationInput ToInput(string variantCode)
            => new(
                AssistanceKind ?? variantCode switch
                {
                    AncillaryVariant.DisabilityAssistance => Messages.Ancillary.Enums.AssistanceKind.DisabilityAssistance,
                    AncillaryVariant.MedicalEquipment => Messages.Ancillary.Enums.AssistanceKind.MedicalEquipment,
                    AncillaryVariant.Bassinet => Messages.Ancillary.Enums.AssistanceKind.Bassinet,
                    AncillaryVariant.UnaccompaniedMinor => Messages.Ancillary.Enums.AssistanceKind.UnaccompaniedMinor,
                    _ => Messages.Ancillary.Enums.AssistanceKind.Wheelchair
                },
                Wheelchair,
                DisabilityAssistance,
                MedicalEquipment,
                Bassinet?.ToInput(),
                UnaccompaniedMinor?.ToInput());
    }
}
