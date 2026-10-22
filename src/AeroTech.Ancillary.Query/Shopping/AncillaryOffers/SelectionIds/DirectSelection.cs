using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.SelectionIds
{
    public static class DirectSelection
    {
        private static readonly IReadOnlySet<string> CoverageFields = new HashSet<string>(StringComparer.Ordinal) { "TravellerRef", "BoundRef", "FlightRef", "ChildRef" };

        public static AncillarySelection? TryBuild(CanonicalAncillaryOfferCandidate candidate, IReadOnlyList<OfferOptionGroup> groups)
        {
            var selection = new AncillarySelection();

            foreach (var field in Chosen(candidate))
            {
                var property = SelectionFields.Find(field.Name);

                if (property is null)
                    return null;

                object? value = field.Type switch
                {
                    SelectionFieldType.Integer when field.Name == "Quantity" && candidate.Quantity.MinPerSelection == candidate.Quantity.MaxPerSelection => candidate.Quantity.MinPerSelection,
                    SelectionFieldType.Code or SelectionFieldType.Reference => SingleChoice(groups, field) is { } choice ? SelectionFields.Parse(property, choice) : null,
                    _ => null
                };

                if (value is null)
                    return null;

                property.SetValue(selection, value);
            }

            return selection;
        }

        public static bool RequiresConsent(CanonicalAncillaryOfferCandidate candidate)
            => Chosen(candidate).Any(field => field.Type == SelectionFieldType.Boolean);

        public static bool HasConsent(CanonicalAncillaryOfferCandidate candidate, AncillarySelection selection)
            => Chosen(candidate)
                .Where(field => field.Type == SelectionFieldType.Boolean)
                .All(field => SelectionFields.Find(field.Name)?.GetValue(selection) is true);

        private static IEnumerable<CustomerSelectionField> Chosen(CanonicalAncillaryOfferCandidate candidate)
            => candidate.Selection.Fields.Where(field => field.Required && !CoverageFields.Contains(field.Name));

        private static string? SingleChoice(IReadOnlyList<OfferOptionGroup> groups, CustomerSelectionField field)
            => groups.SingleOrDefault(group => string.Equals(group.FieldName, SelectionFields.CamelCase(field.Name), StringComparison.Ordinal)) is { SourceState: OptionSourceState.Known, Choices: [{ } only] }
                ? only.Value
                : null;
    }
}
