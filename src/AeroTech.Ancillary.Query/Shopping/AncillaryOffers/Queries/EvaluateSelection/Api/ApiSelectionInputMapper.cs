using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Api
{
    public static class ApiSelectionInputMapper
    {
        public static AncillarySelection ToSelection(ApiAncillarySelectionInput? input)
            => input is null
                ? new AncillarySelection()
                : new AncillarySelection
                {
                    Quantity = input.Quantity,
                    PackageProductRef = input.PackageProductRef,
                    BagRef = input.BagRef,
                    MeasuredWeightKg = input.MeasuredWeightKg,
                    Dimensions = Dimensions(input.Dimensions),
                    EquipmentKind = input.EquipmentKind,
                    WeightKg = input.WeightKg,
                    SeatNumber = input.SeatNumber,
                    Purpose = input.Purpose,
                    TargetCabinId = input.TargetCabinId,
                    QuoteRef = input.QuoteRef,
                    MealCode = input.MealCode,
                    MenuItemRef = input.MenuItemRef,
                    AnimalType = input.AnimalType,
                    OtherAnimalCode = input.OtherAnimalCode,
                    AnimalAgeWeeks = input.AnimalAgeWeeks,
                    CombinedWeightKg = input.CombinedWeightKg,
                    CarrierDimensions = Dimensions(input.CarrierDimensions),
                    DocumentAcknowledgements = input.DocumentAcknowledgements,
                    SizeBracket = input.SizeBracket,
                    AssistanceSsrCode = input.AssistanceSsrCode,
                    EquipmentCode = input.EquipmentCode,
                    EvidenceDocumentRefs = input.EvidenceDocumentRefs,
                    OxygenUnits = input.OxygenUnits,
                    InfantRef = input.InfantRef,
                    GuardianRef = input.GuardianRef,
                    ChildRef = input.ChildRef,
                    GuardianHandoffContactProvided = input.GuardianHandoffContactProvided,
                    GuardianPickupContactProvided = input.GuardianPickupContactProvided,
                    AirportId = input.AirportId,
                    FacilityRef = input.FacilityRef,
                    TimeWithOffset = input.TimeWithOffset,
                    GuestCount = input.GuestCount,
                    OptIn = input.OptIn,
                    PlanCode = input.PlanCode,
                    DeviceCount = input.DeviceCount
                };

        private static SelectedDimensions? Dimensions(ApiSelectedDimensionsInput? input)
            => input is null ? null : new SelectedDimensions(input.LengthCm, input.WidthCm, input.HeightCm);
    }
}
