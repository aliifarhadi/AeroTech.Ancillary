using System.Globalization;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail
{
    public static class OfferSelectionDetailsMapper
    {
        private static readonly IReadOnlySet<string> CoverageFields = new HashSet<string>(StringComparer.Ordinal) { "TravellerRef", "BoundRef", "FlightRef" };

        public static OfferSelectionContractDto SelectionContract(CustomerSelectionContract contract)
            => new(
                contract.Kind.ToString(),
                contract.Fields
                    .Where(field => !CoverageFields.Contains(field.Name))
                    .Select(field => new OfferSelectionFieldDto(CamelCase(field.Name), field.Type.ToString(), field.Required))
                    .ToList());

        public static OfferDetailQuantityDto Quantity(CanonicalAncillaryOfferCandidate candidate)
            => new(
                candidate.Quantity.Unit.ToString(),
                candidate.Quantity.MinPerSelection,
                candidate.Quantity.MaxPerSelection,
                1,
                candidate.Selection.Fields.Any(field => field is { Name: "Quantity", Required: true }));

        public static IReadOnlyList<OfferOptionGroupDto> OptionGroups(
            AncillaryOfferDetailSnapshot detail,
            Func<IReadOnlyList<string>, IReadOnlyList<string>> travellers,
            Func<IReadOnlyList<string>, IReadOnlyList<string>> flights)
        {
            var candidate = detail.Item.Candidate;
            var applicability = new OfferApplicabilityDto(travellers(candidate.Travellers), flights(candidate.FlightRefs), candidate.PortionRefs);
            var preview = candidate.Price is { CurrencyId: { } currencyId, CompleteUnitTotal: { } amount }
                ? new OfferPricePreviewDto(currencyId, amount, PriceIsComplete(candidate.Price))
                : null;
            var sourceVersion = string.Create(CultureInfo.InvariantCulture, $"D{candidate.DefinitionVersionId}-P{candidate.ProvisionId}");

            return detail.OptionGroups
                .Select(group => new OfferOptionGroupDto(
                    group.FieldName,
                    group.SourceState.ToString(),
                    group.Choices
                        .Select(choice => new OfferOptionChoiceDto(
                            OfferItemRef.OptionOf(detail.Item.OfferItemRef, group.FieldName, choice.Value),
                            choice.Value,
                            choice.Label,
                            candidate.Availability.CheckedState.ToString(),
                            applicability,
                            preview,
                            sourceVersion,
                            candidate.ExpiresAtUtc))
                        .ToList()))
                .ToList();
        }

        public static string OptionSourceState(AncillaryOfferDetailSnapshot detail) => OfferOptionCatalog.StateOf(detail.OptionGroups).ToString();

        public static OfferPriceCompletenessDto PriceCompleteness(CanonicalAncillaryOfferCandidate candidate)
            => new(
                PriceIsComplete(candidate.Price),
                candidate.Price.ReasonCodes,
                candidate.Selection.Fields
                    .Where(field => field.Required && !CoverageFields.Contains(field.Name))
                    .Select(field => CamelCase(field.Name))
                    .ToList());

        public static OfferCommercialTermsDto CommercialTerms(OfferCommercialTerms terms)
            => new(
                terms.PurchaseStage.ToString(),
                terms.Disposition.ToString(),
                terms.ReissueRefund.ToString(),
                terms.FormOfRefund?.ToString(),
                terms.ConfirmationRequirement.ToString(),
                terms.BookingMethod.ToString(),
                terms.DocumentRouting.ToString(),
                terms.DocumentType.ToString(),
                terms.Rfic,
                terms.Rfisc,
                terms.CancellationRules
                    .Select(rule => new OfferCancellationRuleDto(
                        rule.RuleRef,
                        rule.ReasonKind.ToString(),
                        rule.WindowFromMinutes,
                        rule.WindowToMinutes,
                        rule.PenaltyKind.ToString(),
                        rule.PenaltyFixedAmount,
                        rule.PenaltyPercentage,
                        rule.PenaltyCurrencyId,
                        rule.PercentageBase?.ToString(),
                        rule.WaiverPermitted,
                        rule.SourceReference))
                    .ToList());

        private static bool PriceIsComplete(PriceAssessment price) => OfferPurchase.IsPriceComplete(price);

        public static string CamelCase(string name) => string.Concat(char.ToLowerInvariant(name[0]), name[1..]);
    }
}
