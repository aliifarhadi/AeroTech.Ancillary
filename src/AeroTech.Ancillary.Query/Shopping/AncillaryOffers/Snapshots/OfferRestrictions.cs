using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
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
}
