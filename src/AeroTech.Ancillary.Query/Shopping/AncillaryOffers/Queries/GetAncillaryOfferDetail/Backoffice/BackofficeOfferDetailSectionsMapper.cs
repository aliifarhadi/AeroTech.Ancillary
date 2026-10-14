using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail.Backoffice
{
    public static class BackofficeOfferDetailSectionsMapper
    {
        public static BackofficeIncludedAllowanceDto Allowance(TravellerFlightBaggageFacts fact)
            => new(
                fact.TravellerRef,
                fact.FlightRef,
                fact.CheckedPieces,
                fact.CheckedWeight,
                EnumValueDto.OfNullable(fact.CheckedWeightUnit),
                fact.CabinPieces,
                fact.CabinWeight,
                EnumValueDto.OfNullable(fact.CabinWeightUnit),
                fact.SourceCompleteness == FactEvidence.Verified);

        public static BackofficeBaggageRuleDto? BaggageRule(OfferBaggageRule? rule)
            => rule is null
                ? null
                : new(
                    rule.FreePieces,
                    rule.FirstExcessPiece,
                    rule.LastExcessPiece,
                    rule.Weight,
                    EnumValueDto.Of(rule.WeightUnit),
                    EnumValueDto.Of(rule.PurchaseApplication),
                    EnumValueDto.OfNullable(rule.ChargeKind),
                    EnumValueDto.OfNullable(rule.AllowanceConcept));

        public static BackofficeSeatRuleDto? SeatRule(OfferSeatRule? rule) => rule is null ? null : new(rule.SeatNumbers, rule.SeatCharacteristicCodes);

        public static BackofficeRestrictionsDto Restrictions(OfferRestrictions restrictions)
            => new(
                restrictions.PassengerTypes.Select(type => EnumValueDto.Of(type)).ToList(),
                restrictions.SalesFrom,
                restrictions.SalesUntil,
                restrictions.ServiceSalesFrom,
                restrictions.ServiceSalesUntil,
                restrictions.PermittedTravelPeriods.Select(period => new BackofficeDatePeriodDto(period.StartDate, period.EndDate)).ToList(),
                restrictions.BlackoutPeriods.Select(period => new BackofficeDatePeriodDto(period.StartDate, period.EndDate)).ToList(),
                restrictions.AdvancePurchase is { } advance
                    ? new BackofficeAdvancePurchaseDto(advance.MinimumPeriod, advance.MaximumPeriod, EnumValueDto.Of(advance.Unit), advance.SameTimeAsTicketed)
                    : null,
                EnumValueDto.Of(restrictions.PurchaseStage));

        public static BackofficeTermsDto Terms(OfferTerms terms)
            => new(
                EnumValueDto.Of(terms.Disposition),
                EnumValueDto.Of(terms.ReissueRefund),
                EnumValueDto.OfNullable(terms.FormOfRefund),
                EnumValueDto.Of(terms.ConfirmationRequirement),
                terms.Commissionable,
                terms.InterlineSettlement);

        public static BackofficeRateDto Rate(OfferPublishedRate rate)
            => new(
                EnumValueDto.OfNullable(rate.PassengerTypeCode),
                rate.AgeFromInclusive,
                rate.AgeToExclusive,
                rate.CurrencyId,
                rate.BaseAmount,
                rate.UnitTotal,
                rate.IsUnitTotalComplete,
                rate.IsApplied);
    }
}
