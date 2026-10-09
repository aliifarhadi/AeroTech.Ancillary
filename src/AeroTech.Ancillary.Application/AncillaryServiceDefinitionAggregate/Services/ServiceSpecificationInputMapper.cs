using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Services
{
    public static class ServiceSpecificationInputMapper
    {
        public static ServiceSpecificationArgs ToArgs(ServiceSpecificationInput input)
            => new(
                input.Baggage is null ? null : ToArgs(input.Baggage),
                input.Seat is null ? null : ToArgs(input.Seat),
                input.Upgrade is null ? null : ToArgs(input.Upgrade),
                input.Meal is null ? null : ToArgs(input.Meal),
                input.Pet is null ? null : ToArgs(input.Pet),
                input.AssistedTravel is null ? null : ToArgs(input.AssistedTravel),
                input.AirportService is null ? null : ToArgs(input.AirportService),
                input.Priority is null ? null : ToArgs(input.Priority),
                input.Connectivity is null ? null : ToArgs(input.Connectivity));

        public static DimensionsCmArgs ToArgs(DimensionsCmInput input)
            => new(
                input.LengthCm,
                input.WidthCm,
                input.HeightCm);

        public static BaggageSpecificationArgs ToArgs(BaggageSpecificationInput input)
            => new(
                input.ChargeKind,
                input.AllowanceConcept,
                input.PackageWeightKg,
                input.MaxKgPerPiece,
                input.WeightFromExclusiveKg,
                input.WeightToInclusiveKg,
                input.MaxSize is null ? null : ToArgs(input.MaxSize),
                input.MaxLinearSumCm,
                input.EquipmentKind,
                input.ChargeCombination);

        public static SeatSpecificationArgs ToArgs(SeatSpecificationInput input)
            => new(
                input.SeatPurpose,
                input.SeatCharacteristicCodes ?? [],
                input.ApplicableCabinIds ?? [],
                input.RequiresExitRowEligibility,
                input.RequiresAdjacentSeat,
                input.ExtraSeatPurpose,
                input.ExtraOccupiedSeatCount,
                input.RequiresExternalTicketAction);

        public static UpgradeSpecificationArgs ToArgs(UpgradeSpecificationInput input)
            => new(
                input.FromCabinId,
                input.ToCabinId,
                input.AllowedUpgradeKind,
                input.RequiresTicketExchange,
                input.EligibleFareFamilyIds ?? []);

        public static MealSpecificationArgs ToArgs(MealSpecificationInput input)
            => new(
                input.MealKind,
                input.MealCode,
                input.MenuItemRef,
                input.DietaryCode,
                input.CateringLeadTimeMinutes,
                input.ExclusiveMealFamilyCode);

        public static PetAnimalArgs ToArgs(PetAnimalInput input)
            => new(
                input.AnimalType,
                input.OtherCode);

        public static PetSizeBracketArgs ToArgs(PetSizeBracketInput input)
            => new(
                input.Code,
                input.WeightFromExclusiveKg,
                input.WeightToInclusiveKg);

        public static PetSpecificationArgs ToArgs(PetSpecificationInput input)
            => new(
                input.TransportMode,
                (input.AllowedAnimalTypes ?? []).Select(ToArgs).ToList(),
                input.MaxCombinedWeightKg,
                input.CarrierDimensionsMaxCm is null ? null! : ToArgs(input.CarrierDimensionsMaxCm),
                input.MinAnimalAgeWeeks,
                input.RequiredDocumentCodes ?? [],
                (input.AllowedHoldAnimalSizeBrackets ?? []).Select(ToArgs).ToList());

        public static WheelchairDetailsArgs ToArgs(WheelchairDetailsInput input)
            => new(
                input.AllowedSsrCodes ?? [],
                input.AssistanceLevelCode,
                input.LeadTimeMinutes);

        public static DisabilityAssistanceDetailsArgs ToArgs(DisabilityAssistanceDetailsInput input)
            => new(
                input.AllowedSsrCodes ?? [],
                input.RequiredCommunicationMethod);

        public static MedicalEquipmentDetailsArgs ToArgs(MedicalEquipmentDetailsInput input)
            => new(
                input.MedicalServiceCode,
                input.RequiresMedicalApproval,
                input.EquipmentKind,
                input.OxygenUnits,
                input.EvidenceTypeCodes ?? []);

        public static BassinetDetailsArgs ToArgs(BassinetDetailsInput input)
            => new(
                input.MaxInfantWeightKg,
                input.MaxInfantAgeMonths,
                input.CompatibleSeatGroups ?? [],
                input.RequiresInfantAndGuardian);

        public static UnaccompaniedMinorDetailsArgs ToArgs(UnaccompaniedMinorDetailsInput input)
            => new(
                input.MinAgeYears,
                input.MaxAgeYearsExclusive,
                input.GuardianContactRequired,
                input.ConnectionPolicy,
                input.AllowedTransitAirportIds ?? []);

        public static AssistedTravelSpecificationArgs ToArgs(AssistedTravelSpecificationInput input)
            => new(
                input.AssistanceKind,
                input.Wheelchair is null ? null : ToArgs(input.Wheelchair),
                input.DisabilityAssistance is null ? null : ToArgs(input.DisabilityAssistance),
                input.MedicalEquipment is null ? null : ToArgs(input.MedicalEquipment),
                input.Bassinet is null ? null : ToArgs(input.Bassinet),
                input.UnaccompaniedMinor is null ? null : ToArgs(input.UnaccompaniedMinor));

        public static AirportServiceSpecificationArgs ToArgs(AirportServiceSpecificationInput input)
            => new(
                input.Kind,
                input.AirportId,
                input.TerminalRef,
                input.FacilityId,
                input.Direction,
                input.ServiceWindowStart,
                input.ServiceWindowEnd,
                input.IanaTimeZone,
                input.VisitDurationMinutes,
                input.MaxGuestsPerPrimary,
                input.IncludedComponentCodes ?? [],
                input.RequiresSpecificAppointment);

        public static PrioritySpecificationArgs ToArgs(PrioritySpecificationInput input)
            => new(
                input.Kind,
                input.PriorityZoneCode,
                input.PriorityGroupCode,
                input.FareBenefitRef,
                input.AirportIds ?? []);

        public static ConnectivitySpecificationArgs ToArgs(ConnectivitySpecificationInput input)
            => new(
                input.PlanKind,
                input.DurationMinutes,
                input.IncludedDataMb,
                input.MaxDevices,
                input.EligibleAircraftIds ?? [],
                input.DeliveryStage,
                input.FulfillmentProviderRef);
    }
}
