using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public interface ISelectionOptions
    {
        string? BagRef => null;

        decimal? MeasuredWeightKg => null;

        SelectedDimensionsInput? Dimensions => null;

        decimal? WeightKg => null;

        PetAnimalType? AnimalType => null;

        string? OtherAnimalCode => null;

        int? AnimalAgeWeeks => null;

        decimal? CombinedWeightKg => null;

        SelectedDimensionsInput? CarrierDimensions => null;

        IReadOnlyList<string>? DocumentAcknowledgements => null;

        string? SizeBracket => null;

        string? AssistanceSsrCode => null;

        string? InfantRef => null;

        string? GuardianRef => null;

        DateTimeOffset? TimeWithOffset => null;

        int? GuestCount => null;

        bool? OptIn => null;

        int? DeviceCount => null;
    }
}
