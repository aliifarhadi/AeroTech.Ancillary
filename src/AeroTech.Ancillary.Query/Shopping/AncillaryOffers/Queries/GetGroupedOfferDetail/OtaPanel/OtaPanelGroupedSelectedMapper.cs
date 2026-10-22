using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail.OtaPanel
{
    public static class OtaPanelGroupedSelectedMapper
    {
        public static OtaPanelGroupedSelectedDto Map(AncillaryProfile profile, AncillarySelection selection)
            => profile switch
            {
                AncillaryProfile.Baggage => new OtaPanelGroupedBaggageSelectedDto(selection.Quantity, selection.PackageProductRef, selection.BagRef, selection.MeasuredWeightKg, Dimensions(selection.Dimensions), selection.EquipmentKind, selection.WeightKg),
                AncillaryProfile.Meal => new OtaPanelGroupedMealSelectedDto(selection.MealCode, selection.MenuItemRef, selection.Quantity),
                AncillaryProfile.Pet => new OtaPanelGroupedPetSelectedDto(selection.AnimalType?.ToString(), selection.OtherAnimalCode, selection.AnimalAgeWeeks, selection.CombinedWeightKg, Dimensions(selection.CarrierDimensions), selection.DocumentAcknowledgements, selection.SizeBracket),
                AncillaryProfile.AssistedTravel => new OtaPanelGroupedAssistedTravelSelectedDto(selection.AssistanceSsrCode, selection.EquipmentCode, selection.EvidenceDocumentRefs, selection.OxygenUnits, selection.InfantRef, selection.GuardianRef, selection.ChildRef, selection.GuardianHandoffContactProvided, selection.GuardianPickupContactProvided),
                AncillaryProfile.AirportService => new OtaPanelGroupedAirportServiceSelectedDto(selection.AirportId, selection.FacilityRef, selection.TimeWithOffset, selection.GuestCount),
                AncillaryProfile.Priority => new OtaPanelGroupedPrioritySelectedDto(selection.OptIn),
                AncillaryProfile.Connectivity => new OtaPanelGroupedConnectivitySelectedDto(selection.PlanCode, selection.DeviceCount),
                AncillaryProfile.Upgrade => new OtaPanelGroupedUpgradeSelectedDto(selection.TargetCabinId, selection.QuoteRef),
                AncillaryProfile.Seat => new OtaPanelGroupedExtraSeatSelectedDto(selection.SeatNumber, selection.Purpose?.ToString()),
                _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null)
            };

        private static OtaPanelGroupedDimensionsDto? Dimensions(SelectedDimensions? dimensions)
            => dimensions is null ? null : new OtaPanelGroupedDimensionsDto(dimensions.LengthCm, dimensions.WidthCm, dimensions.HeightCm);
    }
}
