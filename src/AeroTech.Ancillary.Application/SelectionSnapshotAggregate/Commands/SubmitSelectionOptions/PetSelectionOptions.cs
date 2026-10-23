using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record PetSelectionOptions(
        PetAnimalType? AnimalType,
        string? OtherAnimalCode,
        int? AnimalAgeWeeks,
        decimal? CombinedWeightKg,
        SelectedDimensionsInput? CarrierDimensions,
        IReadOnlyList<string>? DocumentAcknowledgements,
        string? SizeBracket) : ISelectionOptions;
}
