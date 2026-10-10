using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal static class PriceAssessor
    {
        public static PriceAssessment Assess(
            AncillaryProvision provision,
            AncillaryPricing? pricing,
            EvaluationUnit unit,
            UnitTimeline timeline,
            IReadOnlyDictionary<int, int> currencyDecimalPlaces)
        {
            switch (provision.PriceOrigin)
            {
                case PriceOrigin.NotAvailable:
                    return Unpriced(provision, PriceAssessmentStatus.Unavailable, ShoppingReasonCodes.NotAvailable);
                case PriceOrigin.Free:
                    return new PriceAssessment { Origin = PriceOrigin.Free, Status = PriceAssessmentStatus.Complete, IsOrderLevelTotalComplete = true };
                case PriceOrigin.ExternalQuote:
                    return new PriceAssessment
                    {
                        Origin = PriceOrigin.ExternalQuote,
                        Status = PriceAssessmentStatus.QuoteRequired,
                        QuoteProviderKey = provision.QuoteProviderKey,
                        ReasonCodes = [ShoppingReasonCodes.QuoteRequired]
                    };
                case PriceOrigin.Filed:
                    return Filed(provision, pricing, unit, timeline, currencyDecimalPlaces);
                default:
                    return Unpriced(provision, PriceAssessmentStatus.Unavailable, ShoppingReasonCodes.PriceOriginNotSupported);
            }
        }

        private static PriceAssessment Unpriced(AncillaryProvision provision, PriceAssessmentStatus status, params string[] reasonCodes)
            => new() { Origin = provision.PriceOrigin, Status = status, ReasonCodes = reasonCodes };

        private static PriceAssessment Filed(
            AncillaryProvision provision,
            AncillaryPricing? pricing,
            EvaluationUnit unit,
            UnitTimeline timeline,
            IReadOnlyDictionary<int, int> currencyDecimalPlaces)
        {
            var currencyId = unit.Context.CurrencyId;

            if (pricing is null)
                return Unpriced(provision, PriceAssessmentStatus.Unavailable, ShoppingReasonCodes.PricingNotActive);

            if (pricing.PricingUnit is null)
                return Unpriced(provision, PriceAssessmentStatus.Incomplete, ShoppingReasonCodes.PricingUnitNotAssigned);

            var ages = timeline.Ages?.Distinct().ToList();
            var rates = pricing.Rates
                .Where(rate => rate.CurrencyId == currencyId)
                .Where(rate => rate.PassengerTypeCode is null || rate.PassengerTypeCode == unit.Traveller.PassengerTypeCode)
                .ToList();

            if (rates.Any(rate => rate.AgeFromInclusive is not null) && ages is not { Count: 1 })
                return Unpriced(provision, PriceAssessmentStatus.Incomplete, ShoppingReasonCodes.AgeNotVerified) with { PricingRevisionId = pricing.Id };

            rates = rates
                .Where(rate => rate.AgeFromInclusive is null
                               || (ages![0] >= rate.AgeFromInclusive.Value && (rate.AgeToExclusive is null || ages[0] < rate.AgeToExclusive.Value)))
                .ToList();

            if (rates.Count != 1)
                return Unpriced(provision, PriceAssessmentStatus.Unavailable, rates.Count == 0 ? ShoppingReasonCodes.RateNotFound : ShoppingReasonCodes.RateAmbiguous)
                    with { PricingRevisionId = pricing.Id };

            return Priced(pricing, rates[0], unit.Selection?.Quantity, currencyDecimalPlaces);
        }

        private static PriceAssessment Priced(AncillaryPricing pricing, AncillaryPricingRate rate, int? quantity, IReadOnlyDictionary<int, int> currencyDecimalPlaces)
        {
            var reasons = new List<string>();
            var lines = rate.Components.Select(Line).ToList();
            var taxes = lines.Where(line => line.Category == AncillaryPriceLineCategory.Tax).ToList();
            var fees = lines.Where(line => line.Category == AncillaryPriceLineCategory.Fee).ToList();
            var unapplied = fees.Where(line => line.FeeApplicationUnit != FeeApplicationUnit.Item).ToList();

            if (!currencyDecimalPlaces.TryGetValue(rate.CurrencyId, out var decimals))
                reasons.Add(ShoppingReasonCodes.CurrencyScaleUnknown);
            else if (lines.Select(line => line.Amount).Append(rate.BaseAmount).Any(amount => decimal.Round(amount, decimals) != amount))
                reasons.Add(ShoppingReasonCodes.AmountScaleNotAllowed);

            if (taxes.Any(line => line.TaxTreatment is not (TaxTreatment.AddedToBase or TaxTreatment.IncludedInBase)))
                reasons.Add(ShoppingReasonCodes.TaxTreatmentUnknown);

            if (unapplied.Any(line => line.FeeApplicationUnit is null or > FeeApplicationUnit.Ticket))
                reasons.Add(ShoppingReasonCodes.FeeUnitNotSupported);

            var complete = reasons.Count == 0;

            if (complete && unapplied.Count > 0)
                reasons.Add(ShoppingReasonCodes.FeeNotAppliedAtUnitLevel);

            decimal? unitTotal = complete ? rate.UnitTotal.Amount : null;

            return new PriceAssessment
            {
                Origin = PriceOrigin.Filed,
                Status = complete ? PriceAssessmentStatus.Complete : PriceAssessmentStatus.Incomplete,
                CurrencyId = rate.CurrencyId,
                PricingRevisionId = pricing.Id,
                PricingRateId = rate.Id,
                PricingUnit = pricing.PricingUnit,
                BaseAmount = rate.BaseAmount,
                AddedTaxLines = taxes.Where(line => line.TaxTreatment == TaxTreatment.AddedToBase).ToList(),
                IncludedTaxLines = taxes.Where(line => line.TaxTreatment == TaxTreatment.IncludedInBase).ToList(),
                AppliedUnitFeeLines = fees.Where(line => line.FeeApplicationUnit == FeeApplicationUnit.Item).ToList(),
                UnappliedFeeLines = unapplied,
                CompleteUnitTotal = unitTotal,
                RequestedQuantityTotal = unitTotal is not null && quantity is > 0 ? unitTotal.Value * quantity.Value : null,
                IsOrderLevelTotalComplete = complete && unapplied.Count == 0,
                ReasonCodes = reasons
            };
        }

        private static PriceComponentLine Line(AncillaryPriceComponent component)
            => new(
                component.Category,
                component.Code,
                component.CountryId,
                component.StationAirportId,
                component.Amount.Amount,
                component.Amount.CurrencyId,
                component.TaxTreatment,
                component.FeeApplicationUnit);
    }
}
