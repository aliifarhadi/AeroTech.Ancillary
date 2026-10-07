using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class BaggageApplication : ValueObject
    {
        private const decimal WeightUpperBound = 10000000m;

        private BaggageApplication()
        {
        }

        public int? FreePieces { get; private set; }

        public int? FirstExcessPiece { get; private set; }

        public int? LastExcessPiece { get; private set; }

        public decimal? Weight { get; private set; }

        public WeightUnit WeightUnit { get; private set; }

        public BaggageTravelApplication? TravelApplication { get; private set; }

        public BaggagePurchaseApplication PurchaseApplication { get; private set; }

        public BaggageRuleDeference? RuleDeference { get; private set; }

        public static BaggageApplication Create(
            int? freePieces,
            int? firstExcessPiece,
            int? lastExcessPiece,
            decimal? weight,
            WeightUnit weightUnit,
            BaggageTravelApplication? travelApplication,
            BaggagePurchaseApplication purchaseApplication,
            BaggageRuleDeference? ruleDeference)
        {
            Require(freePieces is null or >= 0, nameof(FreePieces));
            Require(firstExcessPiece is null or >= 1, nameof(FirstExcessPiece));
            Require(lastExcessPiece is null or >= 1, nameof(LastExcessPiece));
            Require(
                firstExcessPiece is null || lastExcessPiece is null || firstExcessPiece <= lastExcessPiece,
                nameof(LastExcessPiece));
            Require(
                weight is null || (weight > 0 && weight < WeightUpperBound && weight == decimal.Round(weight.Value, 2)),
                nameof(Weight));
            Require(Enum.IsDefined(weightUnit), nameof(WeightUnit));
            Require(travelApplication is null || Enum.IsDefined(travelApplication.Value), nameof(TravelApplication));
            Require(Enum.IsDefined(purchaseApplication), nameof(PurchaseApplication));
            Require(ruleDeference is null || Enum.IsDefined(ruleDeference.Value), nameof(RuleDeference));

            return new BaggageApplication
            {
                FreePieces = freePieces,
                FirstExcessPiece = firstExcessPiece,
                LastExcessPiece = lastExcessPiece,
                Weight = weight,
                WeightUnit = weightUnit,
                TravelApplication = travelApplication,
                PurchaseApplication = purchaseApplication,
                RuleDeference = ruleDeference
            };
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return FreePieces;
            yield return FirstExcessPiece;
            yield return LastExcessPiece;
            yield return Weight;
            yield return WeightUnit;
            yield return TravelApplication;
            yield return PurchaseApplication;
            yield return RuleDeference;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(BaggageApplication)}.{field}");
        }
    }
}
