using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm
{
    public static class AssistedTravelVariantSpecificationMapper
    {
        public static AssistedTravelVariantSpecificationDto ToDto(AncillaryServiceDefinition definition)
        {
            var assistance = definition.AssistedTravel!.ToArgs();

            return definition.VariantCode switch
            {
                AncillaryVariant.Wheelchair => new WheelchairSpecificationDto(
                    new WheelchairFormDto(
                        assistance.Wheelchair!.AllowedSsrCodes,
                        assistance.Wheelchair.AssistanceLevelCode,
                        assistance.Wheelchair.LeadTimeMinutes)),
                AncillaryVariant.DisabilityAssistance => new DisabilityAssistanceSpecificationDto(
                    new DisabilityAssistanceFormDto(
                        assistance.DisabilityAssistance!.AllowedSsrCodes,
                        assistance.DisabilityAssistance.RequiredCommunicationMethod)),
                AncillaryVariant.MedicalEquipment => new MedicalEquipmentSpecificationDto(
                    new MedicalEquipmentFormDto(
                        assistance.MedicalEquipment!.MedicalServiceCode,
                        assistance.MedicalEquipment.RequiresMedicalApproval,
                        assistance.MedicalEquipment.EquipmentKind,
                        assistance.MedicalEquipment.OxygenUnits,
                        assistance.MedicalEquipment.EvidenceTypeCodes)),
                AncillaryVariant.Bassinet => new BassinetSpecificationDto(
                    new BassinetFormDto(
                        assistance.Bassinet!.MaxInfantWeightKg,
                        assistance.Bassinet.MaxInfantAgeMonths,
                        assistance.Bassinet.CompatibleSeatGroups)),
                _ => new UnaccompaniedMinorSpecificationDto(
                    new UnaccompaniedMinorFormDto(
                        assistance.UnaccompaniedMinor!.MinAgeYears,
                        assistance.UnaccompaniedMinor.MaxAgeYearsExclusive,
                        assistance.UnaccompaniedMinor.ConnectionPolicy,
                        assistance.UnaccompaniedMinor.AllowedTransitAirportIds))
            };
        }
    }
}
