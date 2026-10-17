using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate
{
    public sealed partial class AncillaryProvision
    {
        private const int SingleSeat = 1;

        private static readonly PurchaseStage[] GroundPurchaseStages = [PurchaseStage.PreOrder, PurchaseStage.PostTicketed, PurchaseStage.Both];

        public static IReadOnlyList<CommercialDisposition> AllowedDispositions(AncillaryServiceDefinition definition)
            => definition.VariantCode is AncillaryVariant.FreeSpecialMeal or AncillaryVariant.Wheelchair
                ? [CommercialDisposition.Free, CommercialDisposition.NotAvailable]
                : [CommercialDisposition.Paid, CommercialDisposition.Free, CommercialDisposition.NotAvailable];

        public static IReadOnlyList<PriceOrigin> AllowedPriceOrigins(AncillaryServiceDefinition definition)
            => AllowedDispositions(definition)
                .SelectMany(disposition => disposition switch
                {
                    CommercialDisposition.Paid => PaidPriceOrigins(definition),
                    CommercialDisposition.Free => [PriceOrigin.Free],
                    _ => [PriceOrigin.NotAvailable]
                })
                .ToList();

        public static IReadOnlyList<PurchaseStage> AllowedPurchaseStages(AncillaryServiceDefinition definition)
            => definition.Connectivity is { DeliveryStage: PurchaseStage.OnBoard } ? [PurchaseStage.OnBoard] : GroundPurchaseStages;

        public static AncillaryQuantityUnit? QuantityUnitOf(AncillaryServiceDefinition definition)
            => definition.PricingUnit is { } pricingUnit ? QuantityUnits[pricingUnit] : null;

        public static int? MaxQuantityOf(AncillaryServiceDefinition definition)
            => definition.VariantCode is AncillaryVariant.StandardSeat or AncillaryVariant.PreferredSeat ? SingleSeat : null;

        private static PriceOrigin[] PaidPriceOrigins(AncillaryServiceDefinition definition)
            => definition.Upgrade switch
            {
                { AllowedUpgradeKind: UpgradeKind.FixedAncillary } => [PriceOrigin.Filed],
                not null => [PriceOrigin.ExternalQuote],
                _ => [PriceOrigin.Filed, PriceOrigin.ExternalQuote]
            };
    }
}
