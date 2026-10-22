using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail.OtaPanel
{
    public static class OtaPanelGroupedOfferDetailMapper
    {
        private static readonly IReadOnlySet<string> CoverageFields = new HashSet<string>(StringComparer.Ordinal) { "TravellerRef", "BoundRef", "FlightRef" };

        public static OtaPanelGroupedOfferDetailDto Map(GroupedOfferDetailSnapshot snapshot)
        {
            var candidate = snapshot.Evaluation?.Evaluation.Candidate ?? snapshot.Detail.Item.Candidate;

            return new OtaPanelGroupedOfferDetailDto(
                snapshot.Detail.Item.OfferItemRef,
                candidate.VariantCode,
                candidate.OfferReadiness.ToString(),
                GroupedQuantity.Of(candidate) is { } quantity ? new OtaPanelGroupedQuantityDto(quantity.MinPerSelection, quantity.MaxPerSelection, quantity.Unit.ToString()) : null,
                candidate.Selection.Fields
                    .Where(field => !CoverageFields.Contains(field.Name))
                    .Select(field => new OtaPanelGroupedRequiredFieldDto(OfferSelectionDetailsMapper.CamelCase(field.Name), field.Type.ToString(), field.Required))
                    .ToList(),
                snapshot.Detail.OptionGroups
                    .Select(group => new OtaPanelGroupedOptionGroupDto(group.FieldName, group.SourceState.ToString(), group.Choices.Select(choice => new OtaPanelGroupedOptionChoiceDto(choice.Value, choice.Label)).ToList()))
                    .ToList(),
                snapshot.Evaluation is { } evaluation ? OtaPanelGroupedSelectedMapper.Map(candidate.Profile, evaluation.Selection) : null,
                new OtaPanelGroupedPriceDto(candidate.Price.CurrencyId, candidate.Price.CompleteUnitTotal, OfferPurchase.IsPriceComplete(candidate.Price)),
                snapshot.SelectionId,
                snapshot.ReasonCodes);
        }
    }
}
