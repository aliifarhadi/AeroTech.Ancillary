using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.OtaPanel
{
    public static class OtaPanelOfferDetailSectionsMapper
    {
        public static OtaPanelIncludedAllowanceDto Allowance(TravellerFlightBaggageFacts fact)
            => new(
                fact.TravellerRef,
                fact.FlightRef,
                fact.CheckedPieces,
                fact.CheckedWeight,
                fact.CheckedWeightUnit?.ToString(),
                fact.CabinPieces,
                fact.CabinWeight,
                fact.CabinWeightUnit?.ToString(),
                fact.SourceCompleteness == FactEvidence.Verified);

        public static OtaPanelBaggageRuleDto? BaggageRule(OfferBaggageRule? rule)
            => rule is null
                ? null
                : new(
                    rule.FreePieces,
                    rule.FirstExcessPiece,
                    rule.LastExcessPiece,
                    rule.Weight,
                    rule.WeightUnit.ToString(),
                    rule.PurchaseApplication.ToString(),
                    rule.ChargeKind?.ToString(),
                    rule.AllowanceConcept?.ToString());

        public static OtaPanelSeatRuleDto? SeatRule(OfferSeatRule? rule) => rule is null ? null : new(rule.SeatNumbers, rule.SeatCharacteristicCodes);

        public static OtaPanelRestrictionsDto Restrictions(OfferRestrictions restrictions)
            => new(
                restrictions.PassengerTypes.Select(type => type.ToString()).ToList(),
                restrictions.SalesFrom,
                restrictions.SalesUntil,
                restrictions.ServiceSalesFrom,
                restrictions.ServiceSalesUntil,
                restrictions.PermittedTravelPeriods.Select(period => new OtaPanelDatePeriodDto(period.StartDate, period.EndDate)).ToList(),
                restrictions.BlackoutPeriods.Select(period => new OtaPanelDatePeriodDto(period.StartDate, period.EndDate)).ToList(),
                restrictions.AdvancePurchase is { } advance
                    ? new OtaPanelAdvancePurchaseDto(advance.MinimumPeriod, advance.MaximumPeriod, advance.Unit.ToString(), advance.SameTimeAsTicketed)
                    : null,
                restrictions.PurchaseStage.ToString());

        public static OtaPanelTermsDto Terms(OfferTerms terms)
            => new(
                terms.Disposition.ToString(),
                terms.ReissueRefund.ToString(),
                terms.FormOfRefund?.ToString(),
                terms.ConfirmationRequirement.ToString());

        public static OtaPanelRateDto Rate(OfferPublishedRate rate)
            => new(
                rate.PassengerTypeCode?.ToString(),
                rate.AgeFromInclusive,
                rate.AgeToExclusive,
                rate.CurrencyId,
                rate.BaseAmount,
                rate.UnitTotal,
                rate.IsUnitTotalComplete,
                rate.IsApplied);
    }
}
