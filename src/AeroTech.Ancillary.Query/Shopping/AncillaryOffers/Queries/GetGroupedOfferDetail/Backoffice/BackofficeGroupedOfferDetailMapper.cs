using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail.Backoffice
{
    public static class BackofficeGroupedOfferDetailMapper
    {
        private static readonly IReadOnlySet<string> CoverageFields = new HashSet<string>(StringComparer.Ordinal) { "TravellerRef", "BoundRef", "FlightRef" };

        public static BackofficeGroupedOfferDetailDto Map(GroupedOfferDetailSnapshot snapshot)
        {
            var candidate = snapshot.Evaluation?.Evaluation.Candidate ?? snapshot.Detail.Item.Candidate;

            return new BackofficeGroupedOfferDetailDto(
                snapshot.Detail.Item.OfferItemRef,
                candidate.VariantCode,
                EnumValueDto.Of(candidate.OfferReadiness),
                GroupedQuantity.Of(candidate) is { } quantity ? new BackofficeGroupedQuantityDto(quantity.MinPerSelection, quantity.MaxPerSelection, EnumValueDto.Of(quantity.Unit)) : null,
                candidate.Selection.Fields
                    .Where(field => !CoverageFields.Contains(field.Name))
                    .Select(field => new BackofficeGroupedRequiredFieldDto(OfferSelectionDetailsMapper.CamelCase(field.Name), EnumValueDto.Of(field.Type), field.Required))
                    .ToList(),
                snapshot.Detail.OptionGroups
                    .Select(group => new BackofficeGroupedOptionGroupDto(group.FieldName, EnumValueDto.Of(group.SourceState), group.Choices.Select(choice => new BackofficeGroupedOptionChoiceDto(choice.Value, choice.Label)).ToList()))
                    .ToList(),
                snapshot.Evaluation is { } evaluation ? BackofficeGroupedSelectedMapper.Map(candidate.Profile, evaluation.Selection) : null,
                new BackofficeGroupedPriceDto(candidate.Price.CurrencyId, candidate.Price.CompleteUnitTotal, OfferPurchase.IsPriceComplete(candidate.Price)),
                snapshot.SelectionId,
                snapshot.ReasonCodes);
        }
    }
}
