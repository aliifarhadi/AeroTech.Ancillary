using System.Text.Json.Nodes;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Options;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public static class SelectionOptionsMapper
    {
        public static SelectionOptionsDto<IReadOnlyDictionary<string, SelectionFieldDto>>? Options(AncillaryOfferRow row)
            => row.Options is { } fields ? ByFamily(row.Candidate.Profile, Fields(fields)) : null;

        public static SelectedUnitDto? Selected(AncillaryOfferRow row)
            => row.Selected is { } line ? Selected(row.Candidate.Profile, line.Quantity, line.SelectedJson) : null;

        public static SelectedUnitDto Selected(AncillaryProfile family, int quantity, string? selectedJson)
            => new(quantity, selectedJson is null ? null : ByFamily(family, JsonNode.Parse(selectedJson)!.AsObject()));

        private static SelectionOptionsDto<TValue> ByFamily<TValue>(AncillaryProfile family, TValue value)
            where TValue : class
            => family switch
            {
                AncillaryProfile.Baggage => new SelectionOptionsDto<TValue>(Baggage: value),
                AncillaryProfile.Pet => new SelectionOptionsDto<TValue>(Pet: value),
                AncillaryProfile.AssistedTravel => new SelectionOptionsDto<TValue>(AssistedTravel: value),
                AncillaryProfile.AirportService => new SelectionOptionsDto<TValue>(AirportService: value),
                AncillaryProfile.Priority => new SelectionOptionsDto<TValue>(Priority: value),
                AncillaryProfile.Connectivity => new SelectionOptionsDto<TValue>(Connectivity: value),
                _ => throw new ArgumentOutOfRangeException(nameof(family), family, null)
            };

        private static IReadOnlyDictionary<string, SelectionFieldDto> Fields(IReadOnlyList<SelectionFieldDescriptor> fields)
            => fields.ToDictionary(field => field.Name, Field, StringComparer.Ordinal);

        private static SelectionFieldDto Field(SelectionFieldDescriptor field)
            => new(
                field.Control,
                field.Required,
                field.Choices?.Select(choice => new SelectionFieldChoiceDto(choice.Value, choice.Label)).ToList(),
                field.Unit,
                field.Min,
                field.ExclusiveMin,
                field.Max,
                field.Fields is { } parts ? Fields(parts) : null);
    }
}
