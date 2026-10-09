using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById
{
    public static class ServiceSpecificationMapper
    {
        public static ServiceSpecificationDto ToDto(ServiceSpecificationArgs args)
            => new(
                args.Baggage is null ? null : ToDto(args.Baggage),
                args.Seat is null ? null : ToDto(args.Seat),
                args.Upgrade is null ? null : ToDto(args.Upgrade),
                args.Meal is null ? null : ToDto(args.Meal),
                args.Pet is null ? null : ToDto(args.Pet),
                args.AssistedTravel is null ? null : ToDto(args.AssistedTravel),
                args.AirportService is null ? null : ToDto(args.AirportService),
                args.Priority is null ? null : ToDto(args.Priority),
                args.Connectivity is null ? null : ToDto(args.Connectivity));

        public static DimensionsCmDto ToDto(DimensionsCmArgs args)
            => new(
                args.LengthCm,
                args.WidthCm,
                args.HeightCm);

        public static BaggageSpecificationDto ToDto(BaggageSpecificationArgs args)
            => new(
                EnumValueDto.Of(args.ChargeKind),
                EnumValueDto.OfNullable(args.AllowanceConcept),
                args.PackageWeightKg,
                args.MaxKgPerPiece,
                args.WeightFromExclusiveKg,
                args.WeightToInclusiveKg,
                args.MaxSize is null ? null : ToDto(args.MaxSize),
                args.MaxLinearSumCm,
                args.EquipmentKind,
                EnumValueDto.OfNullable(args.ChargeCombination));

        public static SeatSpecificationDto ToDto(SeatSpecificationArgs args)
            => new(
                EnumValueDto.Of(args.SeatPurpose),
                args.SeatCharacteristicCodes ?? [],
                args.ApplicableCabinIds ?? [],
                args.RequiresExitRowEligibility,
                args.RequiresAdjacentSeat,
                EnumValueDto.OfNullable(args.ExtraSeatPurpose),
                args.ExtraOccupiedSeatCount,
                args.RequiresExternalTicketAction);

        public static UpgradeSpecificationDto ToDto(UpgradeSpecificationArgs args)
            => new(
                args.FromCabinId,
                args.ToCabinId,
                EnumValueDto.Of(args.AllowedUpgradeKind),
                args.RequiresTicketExchange,
                args.EligibleFareFamilyIds ?? []);

        public static MealSpecificationDto ToDto(MealSpecificationArgs args)
            => new(
                EnumValueDto.Of(args.MealKind),
                args.MealCode,
                args.MenuItemRef,
                args.DietaryCode,
                args.CateringLeadTimeMinutes,
                args.ExclusiveMealFamilyCode);

        public static PetAnimalDto ToDto(PetAnimalArgs args)
            => new(
                EnumValueDto.Of(args.AnimalType),
                args.OtherCode);

        public static PetSizeBracketDto ToDto(PetSizeBracketArgs args)
            => new(
                args.Code,
                args.WeightFromExclusiveKg,
                args.WeightToInclusiveKg);

        public static PetSpecificationDto ToDto(PetSpecificationArgs args)
            => new(
                EnumValueDto.Of(args.TransportMode),
                (args.AllowedAnimalTypes ?? []).Select(ToDto).ToList(),
                args.MaxCombinedWeightKg,
                args.CarrierDimensionsMaxCm is null ? null! : ToDto(args.CarrierDimensionsMaxCm),
                args.MinAnimalAgeWeeks,
                args.RequiredDocumentCodes ?? [],
                (args.AllowedHoldAnimalSizeBrackets ?? []).Select(ToDto).ToList());

        public static WheelchairDetailsDto ToDto(WheelchairDetailsArgs args)
            => new(
                args.AllowedSsrCodes ?? [],
                args.AssistanceLevelCode,
                args.LeadTimeMinutes);

        public static DisabilityAssistanceDetailsDto ToDto(DisabilityAssistanceDetailsArgs args)
            => new(
                args.AllowedSsrCodes ?? [],
                EnumValueDto.OfNullable(args.RequiredCommunicationMethod));

        public static MedicalEquipmentDetailsDto ToDto(MedicalEquipmentDetailsArgs args)
            => new(
                args.MedicalServiceCode,
                args.RequiresMedicalApproval,
                EnumValueDto.Of(args.EquipmentKind),
                args.OxygenUnits,
                args.EvidenceTypeCodes ?? []);

        public static BassinetDetailsDto ToDto(BassinetDetailsArgs args)
            => new(
                args.MaxInfantWeightKg,
                args.MaxInfantAgeMonths,
                args.CompatibleSeatGroups ?? [],
                args.RequiresInfantAndGuardian);

        public static UnaccompaniedMinorDetailsDto ToDto(UnaccompaniedMinorDetailsArgs args)
            => new(
                args.MinAgeYears,
                args.MaxAgeYearsExclusive,
                args.GuardianContactRequired,
                EnumValueDto.Of(args.ConnectionPolicy),
                args.AllowedTransitAirportIds ?? []);

        public static AssistedTravelSpecificationDto ToDto(AssistedTravelSpecificationArgs args)
            => new(
                EnumValueDto.Of(args.AssistanceKind),
                args.Wheelchair is null ? null : ToDto(args.Wheelchair),
                args.DisabilityAssistance is null ? null : ToDto(args.DisabilityAssistance),
                args.MedicalEquipment is null ? null : ToDto(args.MedicalEquipment),
                args.Bassinet is null ? null : ToDto(args.Bassinet),
                args.UnaccompaniedMinor is null ? null : ToDto(args.UnaccompaniedMinor));

        public static AirportServiceSpecificationDto ToDto(AirportServiceSpecificationArgs args)
            => new(
                EnumValueDto.Of(args.Kind),
                args.AirportId,
                args.TerminalRef,
                args.FacilityId,
                EnumValueDto.Of(args.Direction),
                args.ServiceWindowStart,
                args.ServiceWindowEnd,
                args.IanaTimeZone,
                args.VisitDurationMinutes,
                args.MaxGuestsPerPrimary,
                args.IncludedComponentCodes ?? [],
                args.RequiresSpecificAppointment);

        public static PrioritySpecificationDto ToDto(PrioritySpecificationArgs args)
            => new(
                EnumValueDto.Of(args.Kind),
                args.PriorityZoneCode,
                args.PriorityGroupCode,
                args.FareBenefitRef,
                args.AirportIds ?? []);

        public static ConnectivitySpecificationDto ToDto(ConnectivitySpecificationArgs args)
            => new(
                EnumValueDto.Of(args.PlanKind),
                args.DurationMinutes,
                args.IncludedDataMb,
                args.MaxDevices,
                args.EligibleAircraftIds ?? [],
                EnumValueDto.Of(args.DeliveryStage),
                args.FulfillmentProviderRef);
    }
}
