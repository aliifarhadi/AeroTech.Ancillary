using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection
{
    public static class OfferSelectionResultsMapper
    {
        public static IReadOnlyList<OfferSelectedOptionDto> SelectedOptions(AncillarySelectionSnapshot selection)
            => selection.OptionGroups
                .Select(group => (Group: group, Value: SelectionFieldValues.Of(selection.Selection, group.FieldName)))
                .Where(row => row.Value is not null)
                .Select(row => new OfferSelectedOptionDto(
                    row.Group.FieldName,
                    row.Value!,
                    row.Group.Choices.Any(choice => string.Equals(choice.Value, row.Value, StringComparison.Ordinal))
                        ? OfferItemRef.OptionOf(selection.Item.OfferItemRef, row.Group.FieldName, row.Value!)
                        : null))
                .ToList();

        public static OfferBeneficiaryDto Beneficiary(AncillarySelectionSnapshot selection, Func<IReadOnlyList<string>, IReadOnlyList<string>> travellers)
            => new(travellers(selection.Evaluation.Candidate.Travellers));

        public static OfferSelectionCoverageDto Coverage(AncillarySelectionSnapshot selection, Func<IReadOnlyList<string>, IReadOnlyList<string>> flights)
        {
            var candidate = selection.Evaluation.Candidate;

            return new OfferSelectionCoverageDto(candidate.Scope.CoverageScope.ToString(), flights(candidate.FlightRefs), candidate.PortionRefs);
        }

        public static OfferExactPriceDto ExactPrice(AncillarySelectionSnapshot selection)
            => ExactPrice(selection.Evaluation.Candidate.Price, OfferPurchase.CanPurchase(selection.Evaluation));

        public static OfferExactPriceDto ExactPrice(PriceAssessment price, bool canPurchase)
        {
            var lineTotal = canPurchase ? price.RequestedQuantityTotal : null;

            return new OfferExactPriceDto(
                price.Origin.ToString(),
                price.PricingUnit?.ToString(),
                price.CurrencyId,
                price.FiledCurrencyId,
                price.Units,
                null,
                price.BaseAmount,
                price.AddedTaxLines.Select(Line).ToList(),
                price.IncludedTaxLines.Select(Line).ToList(),
                price.AppliedUnitFeeLines.Select(Line).ToList(),
                price.UnappliedFeeLines.Select(Line).ToList(),
                price.CompleteUnitTotal,
                lineTotal,
                price.RateOfExchange is { } rate
                    ? new OfferFxEvidenceDto(rate.RateOfExchangeTableId, rate.FromCurrencyId, rate.ToCurrencyId, rate.Rate, rate.ToCurrencyDecimalPlaces)
                    : null,
                lineTotal is not null,
                price.ReasonCodes);
        }

        public static OfferPriceLineDto Line(PriceComponentLine line)
            => new(line.Code, line.Amount, line.CurrencyId, line.CountryId, line.StationAirportId, line.FeeApplicationUnit?.ToString(), line.TaxTreatment?.ToString());
    }
}
