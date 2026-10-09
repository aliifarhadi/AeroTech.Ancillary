using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionBaggageApplicationArgs(
        int? FreePieces,
        int? FirstExcessPiece,
        int? LastExcessPiece,
        decimal? Weight,
        WeightUnit WeightUnit,
        BaggageTravelApplication? TravelApplication,
        BaggagePurchaseApplication PurchaseApplication,
        BaggageRuleDeference? RuleDeference,
        BaggageChargeKind? ChargeKind = null,
        BaggageAllowanceConcept? AllowanceConcept = null);
}
