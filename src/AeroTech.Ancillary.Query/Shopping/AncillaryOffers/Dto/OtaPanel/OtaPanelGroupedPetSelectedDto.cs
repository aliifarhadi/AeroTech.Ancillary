namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedPetSelectedDto(
        string? AnimalType,
        string? OtherAnimalCode,
        int? AnimalAgeWeeks,
        decimal? CombinedWeightKg,
        OtaPanelGroupedDimensionsDto? CarrierDimensions,
        IReadOnlyList<string>? DocumentAcknowledgements,
        string? SizeBracket) : OtaPanelGroupedSelectedDto;
}
