using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.OtaPanel
{
    public static class OtaPanelServiceSpecificationMapper
    {
        public static OtaPanelServiceSpecificationDto ToDto(ServiceSpecificationArgs args)
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

        public static OtaPanelDimensionsCmDto ToDto(DimensionsCmArgs args)
            => new(
                args.LengthCm,
                args.WidthCm,
                args.HeightCm);

        public static OtaPanelBaggageSpecificationDto ToDto(BaggageSpecificationArgs args)
            => new(
                args.ChargeKind.ToString(),
                args.AllowanceConcept?.ToString(),
                args.PackageWeightKg,
                args.MaxKgPerPiece,
                args.WeightFromExclusiveKg,
                args.WeightToInclusiveKg,
                args.MaxSize is null ? null : ToDto(args.MaxSize),
                args.MaxLinearSumCm,
                args.EquipmentKind,
                args.ChargeCombination?.ToString());

        public static OtaPanelSeatSpecificationDto ToDto(SeatSpecificationArgs args)
            => new(
                args.SeatPurpose.ToString(),
                args.SeatCharacteristicCodes ?? [],
                args.ApplicableCabinIds ?? [],
                args.RequiresExitRowEligibility,
                args.RequiresAdjacentSeat,
                args.ExtraSeatPurpose?.ToString(),
                args.ExtraOccupiedSeatCount,
                args.RequiresExternalTicketAction);

        public static OtaPanelUpgradeSpecificationDto ToDto(UpgradeSpecificationArgs args)
            => new(
                args.FromCabinId,
                args.ToCabinId,
                args.AllowedUpgradeKind.ToString(),
                args.RequiresTicketExchange,
                args.EligibleFareFamilyIds ?? []);

        public static OtaPanelMealSpecificationDto ToDto(MealSpecificationArgs args)
            => new(
                args.MealKind.ToString(),
                args.MealCode,
                args.MenuItemRef,
                args.DietaryCode,
                args.CateringLeadTimeMinutes,
                args.ExclusiveMealFamilyCode);

        public static OtaPanelPetAnimalDto ToDto(PetAnimalArgs args)
            => new(
                args.AnimalType.ToString(),
                args.OtherCode);

        public static OtaPanelPetSizeBracketDto ToDto(PetSizeBracketArgs args)
            => new(
                args.Code,
                args.WeightFromExclusiveKg,
                args.WeightToInclusiveKg);

        public static OtaPanelPetSpecificationDto ToDto(PetSpecificationArgs args)
            => new(
                args.TransportMode.ToString(),
                (args.AllowedAnimalTypes ?? []).Select(ToDto).ToList(),
                args.MaxCombinedWeightKg,
                args.CarrierDimensionsMaxCm is null ? null! : ToDto(args.CarrierDimensionsMaxCm),
                args.MinAnimalAgeWeeks,
                args.RequiredDocumentCodes ?? [],
                (args.AllowedHoldAnimalSizeBrackets ?? []).Select(ToDto).ToList());

        public static OtaPanelWheelchairDetailsDto ToDto(WheelchairDetailsArgs args)
            => new(
                args.AllowedSsrCodes ?? [],
                args.AssistanceLevelCode,
                args.LeadTimeMinutes);

        public static OtaPanelDisabilityAssistanceDetailsDto ToDto(DisabilityAssistanceDetailsArgs args)
            => new(
                args.AllowedSsrCodes ?? [],
                args.RequiredCommunicationMethod?.ToString());

        public static OtaPanelMedicalEquipmentDetailsDto ToDto(MedicalEquipmentDetailsArgs args)
            => new(
                args.MedicalServiceCode,
                args.RequiresMedicalApproval,
                args.EquipmentKind.ToString(),
                args.OxygenUnits,
                args.EvidenceTypeCodes ?? []);

        public static OtaPanelBassinetDetailsDto ToDto(BassinetDetailsArgs args)
            => new(
                args.MaxInfantWeightKg,
                args.MaxInfantAgeMonths,
                args.CompatibleSeatGroups ?? [],
                args.RequiresInfantAndGuardian);

        public static OtaPanelUnaccompaniedMinorDetailsDto ToDto(UnaccompaniedMinorDetailsArgs args)
            => new(
                args.MinAgeYears,
                args.MaxAgeYearsExclusive,
                args.GuardianContactRequired,
                args.ConnectionPolicy.ToString(),
                args.AllowedTransitAirportIds ?? []);

        public static OtaPanelAssistedTravelSpecificationDto ToDto(AssistedTravelSpecificationArgs args)
            => new(
                args.AssistanceKind.ToString(),
                args.Wheelchair is null ? null : ToDto(args.Wheelchair),
                args.DisabilityAssistance is null ? null : ToDto(args.DisabilityAssistance),
                args.MedicalEquipment is null ? null : ToDto(args.MedicalEquipment),
                args.Bassinet is null ? null : ToDto(args.Bassinet),
                args.UnaccompaniedMinor is null ? null : ToDto(args.UnaccompaniedMinor));

        public static OtaPanelAirportServiceSpecificationDto ToDto(AirportServiceSpecificationArgs args)
            => new(
                args.Kind.ToString(),
                args.AirportId,
                args.TerminalRef,
                args.FacilityId,
                args.Direction.ToString(),
                args.ServiceWindowStart,
                args.ServiceWindowEnd,
                args.IanaTimeZone,
                args.VisitDurationMinutes,
                args.MaxGuestsPerPrimary,
                args.IncludedComponentCodes ?? [],
                args.RequiresSpecificAppointment);

        public static OtaPanelPrioritySpecificationDto ToDto(PrioritySpecificationArgs args)
            => new(
                args.Kind.ToString(),
                args.PriorityZoneCode,
                args.PriorityGroupCode,
                args.FareBenefitRef,
                args.AirportIds ?? []);

        public static OtaPanelConnectivitySpecificationDto ToDto(ConnectivitySpecificationArgs args)
            => new(
                args.PlanKind.ToString(),
                args.DurationMinutes,
                args.IncludedDataMb,
                args.MaxDevices,
                args.EligibleAircraftIds ?? [],
                args.DeliveryStage.ToString(),
                args.FulfillmentProviderRef);
    }
}
