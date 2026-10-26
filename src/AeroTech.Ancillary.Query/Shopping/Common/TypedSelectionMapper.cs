using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.Shopping.Common
{
    public static class TypedSelectionMapper
    {
        public static AncillarySelection ToSelection(ISelectionOptions options)
            => new()
            {
                BagRef = options.BagRef,
                MeasuredWeightKg = options.MeasuredWeightKg,
                Dimensions = Dimensions(options.Dimensions),
                WeightKg = options.WeightKg,
                AnimalType = options.AnimalType,
                OtherAnimalCode = options.OtherAnimalCode,
                AnimalAgeWeeks = options.AnimalAgeWeeks,
                CombinedWeightKg = options.CombinedWeightKg,
                CarrierDimensions = Dimensions(options.CarrierDimensions),
                DocumentAcknowledgements = options.DocumentAcknowledgements,
                SizeBracket = options.SizeBracket,
                AssistanceSsrCode = options.AssistanceSsrCode,
                InfantRef = options.InfantRef,
                GuardianRef = options.GuardianRef,
                TimeWithOffset = options.TimeWithOffset,
                GuestCount = options.GuestCount,
                OptIn = options.OptIn,
                DeviceCount = options.DeviceCount
            };

        private static SelectedDimensions? Dimensions(SelectedDimensionsInput? dimensions)
            => dimensions is null ? null : new SelectedDimensions(dimensions.LengthCm, dimensions.WidthCm, dimensions.HeightCm);
    }
}
