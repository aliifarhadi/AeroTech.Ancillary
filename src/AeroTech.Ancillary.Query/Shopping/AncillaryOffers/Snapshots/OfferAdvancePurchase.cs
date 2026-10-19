using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record OfferAdvancePurchase(int MinimumPeriod, int? MaximumPeriod, TimeUnit Unit, bool SameTimeAsTicketed);
}
