using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail.Backoffice
{
    public static class BackofficeGroupedSelectedMapper
    {
        public static BackofficeGroupedSelectedDto Map(AncillaryProfile profile, AncillarySelection selection)
            => profile switch
            {
                AncillaryProfile.Baggage => new BackofficeGroupedBaggageSelectedDto(selection.Quantity, selection.PackageProductRef, selection.BagRef, selection.MeasuredWeightKg, Dimensions(selection.Dimensions), selection.EquipmentKind, selection.WeightKg),
                AncillaryProfile.Meal => new BackofficeGroupedMealSelectedDto(selection.MealCode, selection.MenuItemRef, selection.Quantity),
                AncillaryProfile.Pet => new BackofficeGroupedPetSelectedDto(EnumValueDto.OfNullable(selection.AnimalType), selection.OtherAnimalCode, selection.AnimalAgeWeeks, selection.CombinedWeightKg, Dimensions(selection.CarrierDimensions), selection.DocumentAcknowledgements, selection.SizeBracket),
                AncillaryProfile.AssistedTravel => new BackofficeGroupedAssistedTravelSelectedDto(selection.AssistanceSsrCode, selection.EquipmentCode, selection.EvidenceDocumentRefs, selection.OxygenUnits, selection.InfantRef, selection.GuardianRef, selection.ChildRef, selection.GuardianHandoffContactProvided, selection.GuardianPickupContactProvided),
                AncillaryProfile.AirportService => new BackofficeGroupedAirportServiceSelectedDto(selection.AirportId, selection.FacilityRef, selection.TimeWithOffset, selection.GuestCount),
                AncillaryProfile.Priority => new BackofficeGroupedPrioritySelectedDto(selection.OptIn),
                AncillaryProfile.Connectivity => new BackofficeGroupedConnectivitySelectedDto(selection.PlanCode, selection.DeviceCount),
                AncillaryProfile.Upgrade => new BackofficeGroupedUpgradeSelectedDto(selection.TargetCabinId, selection.QuoteRef),
                AncillaryProfile.Seat => new BackofficeGroupedExtraSeatSelectedDto(selection.SeatNumber, EnumValueDto.OfNullable(selection.Purpose)),
                _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, null)
            };

        private static BackofficeGroupedDimensionsDto? Dimensions(SelectedDimensions? dimensions)
            => dimensions is null ? null : new BackofficeGroupedDimensionsDto(dimensions.LengthCm, dimensions.WidthCm, dimensions.HeightCm);
    }
}
