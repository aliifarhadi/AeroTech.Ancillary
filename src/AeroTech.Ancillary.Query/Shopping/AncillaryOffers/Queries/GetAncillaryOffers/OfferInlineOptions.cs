using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public static class OfferInlineOptions
    {
        public const int MaxInlineOptionsPerGroup = 12;
        public const string Inline = "Inline";
        public const string DetailsOnDemand = "DetailsOnDemand";
        public const string QuoteRequired = "QuoteRequired";
        public const string DocumentsRequired = "DocumentsRequired";
        public const string AppointmentRequired = "AppointmentRequired";
        public const string TypedInputRequired = "TypedInputRequired";
        public const string OptionSourceUnknown = "OptionSourceUnknown";
        public const string NoOptionAvailable = "NoOptionAvailable";
        public const string OptionsDeferred = "OptionsDeferred";

        private static readonly IReadOnlySet<string> CoverageFields = new HashSet<string>(StringComparer.Ordinal) { "TravellerRef", "BoundRef", "FlightRef" };
        private static readonly IReadOnlySet<string> DocumentFields = new HashSet<string>(StringComparer.Ordinal) { "DocumentAcknowledgements", "EvidenceDocumentRefs" };

        public static IReadOnlyList<OfferInlineOptionGroupDto> Groups(
            string offerItemRef,
            CanonicalAncillaryOfferCandidate candidate,
            IReadOnlyList<OfferOptionGroup> groups,
            Func<IReadOnlyList<string>, IReadOnlyList<string>> travellers,
            Func<IReadOnlyList<string>, IReadOnlyList<string>> flights)
            => groups
                .Select(group => IncludesChoices(group)
                    ? new OfferInlineOptionGroupDto(group.FieldName, group.SourceState.ToString(), OfferSelectionDetailsMapper.Choices(offerItemRef, candidate, group, travellers, flights), group.Choices.Count, true)
                    : new OfferInlineOptionGroupDto(group.FieldName, group.SourceState.ToString(), [], group.Choices.Count, false))
                .ToList();

        public static bool IncludesChoices(OfferOptionGroup group) => group is { SourceState: OptionSourceState.Known, Choices.Count: <= MaxInlineOptionsPerGroup };

        public static OfferInteractionDto Interaction(CanonicalAncillaryOfferCandidate candidate, IReadOnlyList<OfferOptionGroup> groups)
        {
            var reasons = new List<string>();

            if (candidate.Selection.Kind == SelectionKind.ExternalQuote || candidate.Price.Origin == PriceOrigin.ExternalQuote)
                reasons.Add(QuoteRequired);

            foreach (var field in candidate.Selection.Fields.Where(field => field.Required && !CoverageFields.Contains(field.Name)))
            {
                var group = groups.FirstOrDefault(row => string.Equals(row.FieldName, OfferSelectionDetailsMapper.CamelCase(field.Name), StringComparison.Ordinal));

                if (Reason(field, group) is { } reason && !reasons.Contains(reason, StringComparer.Ordinal))
                    reasons.Add(reason);
            }

            return new OfferInteractionDto(reasons.Count == 0 ? Inline : DetailsOnDemand, reasons.Count > 0, reasons);
        }

        private static string? Reason(CustomerSelectionField field, OfferOptionGroup? group)
            => DocumentFields.Contains(field.Name)
                ? DocumentsRequired
                : field.Type switch
                {
                    SelectionFieldType.Integer or SelectionFieldType.Boolean => null,
                    SelectionFieldType.DateTimeWithOffset => AppointmentRequired,
                    SelectionFieldType.Code or SelectionFieldType.CodeSet or SelectionFieldType.Reference => group switch
                    {
                        null => TypedInputRequired,
                        { SourceState: OptionSourceState.Unknown } => OptionSourceUnknown,
                        { Choices.Count: 0 } => NoOptionAvailable,
                        { Choices.Count: > MaxInlineOptionsPerGroup } => OptionsDeferred,
                        _ => null
                    },
                    _ => TypedInputRequired
                };
    }
}
