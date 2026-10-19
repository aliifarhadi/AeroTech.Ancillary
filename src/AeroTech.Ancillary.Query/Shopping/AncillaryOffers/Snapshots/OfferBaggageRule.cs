using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record OfferBaggageRule(
        int? FreePieces,
        int? FirstExcessPiece,
        int? LastExcessPiece,
        decimal? Weight,
        WeightUnit WeightUnit,
        BaggagePurchaseApplication PurchaseApplication,
        BaggageChargeKind? ChargeKind,
        BaggageAllowanceConcept? AllowanceConcept);
}
