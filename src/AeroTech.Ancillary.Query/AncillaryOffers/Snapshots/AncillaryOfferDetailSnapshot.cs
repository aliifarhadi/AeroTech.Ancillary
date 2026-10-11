using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Snapshots
{
    public sealed record AncillaryOfferDetailSnapshot(
        AncillaryShoppingSnapshot Shopping,
        AncillaryOfferItem Item,
        ServiceSpecificationArgs Specification,
        IReadOnlyList<TravellerFlightBaggageFacts> IncludedAllowance,
        OfferBaggageRule? BaggageRule,
        OfferSeatRule? SeatRule,
        OfferRestrictions Restrictions,
        OfferTerms Terms,
        IReadOnlyList<OfferPublishedRate> PublishedRates);

    public sealed record OfferBaggageRule(
        int? FreePieces,
        int? FirstExcessPiece,
        int? LastExcessPiece,
        decimal? Weight,
        WeightUnit WeightUnit,
        BaggagePurchaseApplication PurchaseApplication,
        BaggageChargeKind? ChargeKind,
        BaggageAllowanceConcept? AllowanceConcept);

    public sealed record OfferSeatRule(IReadOnlyList<string> SeatNumbers, IReadOnlyList<string> SeatCharacteristicCodes);

    public sealed record OfferRestrictions(
        IReadOnlyList<PassengerTypeCode> PassengerTypes,
        DateTimeOffset? SalesFrom,
        DateTimeOffset? SalesUntil,
        DateOnly? ServiceSalesFrom,
        DateOnly? ServiceSalesUntil,
        IReadOnlyList<OfferDatePeriod> PermittedTravelPeriods,
        IReadOnlyList<OfferDatePeriod> BlackoutPeriods,
        OfferAdvancePurchase? AdvancePurchase,
        PurchaseStage PurchaseStage);

    public sealed record OfferDatePeriod(DateOnly StartDate, DateOnly EndDate);

    public sealed record OfferAdvancePurchase(int MinimumPeriod, int? MaximumPeriod, TimeUnit Unit, bool SameTimeAsTicketed);

    public sealed record OfferTerms(
        CommercialDisposition Disposition,
        ReissueRefundPolicy ReissueRefund,
        FormOfRefund? FormOfRefund,
        bool Commissionable,
        bool InterlineSettlement,
        ConfirmationRequirement ConfirmationRequirement);

    public sealed record OfferPublishedRate(
        PassengerTypeCode? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        int CurrencyId,
        decimal BaseAmount,
        decimal UnitTotal,
        bool IsUnitTotalComplete,
        bool IsApplied);
}
