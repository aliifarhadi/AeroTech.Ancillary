using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionBaggageApplicationRule : Entity<long>
    {
        private const decimal WeightUpperBound = 10000000m;

        private ProvisionBaggageApplicationRule()
        {
        }

        private ProvisionBaggageApplicationRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public int? FreePieces { get; private set; }

        public int? FirstExcessPiece { get; private set; }

        public int? LastExcessPiece { get; private set; }

        public decimal? Weight { get; private set; }

        public WeightUnit WeightUnit { get; private set; }

        public BaggageTravelApplication? TravelApplication { get; private set; }

        public BaggagePurchaseApplication PurchaseApplication { get; private set; }

        public BaggageRuleDeference? RuleDeference { get; private set; }

        public BaggageChargeKind? ChargeKind { get; private set; }

        public BaggageAllowanceConcept? AllowanceConcept { get; private set; }

        internal static Func<ProvisionBaggageApplicationRule?> Plan(
            ProvisionBaggageApplicationRule? stored,
            long ancillaryProvisionId,
            ProvisionBaggageApplicationArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null)
                return () => null;

            Require(args.FreePieces is null or >= 0, nameof(FreePieces));
            Require(args.FirstExcessPiece is null or >= 1, nameof(FirstExcessPiece));
            Require(args.LastExcessPiece is null or >= 1, nameof(LastExcessPiece));
            Require(
                args.FirstExcessPiece is null || args.LastExcessPiece is null || args.FirstExcessPiece <= args.LastExcessPiece,
                nameof(LastExcessPiece));
            Require(
                args.Weight is null || (args.Weight > 0 && args.Weight < WeightUpperBound && args.Weight == decimal.Round(args.Weight.Value, 2)),
                nameof(Weight));
            Require(Enum.IsDefined(args.WeightUnit), nameof(WeightUnit));
            Require(args.TravelApplication is null || Enum.IsDefined(args.TravelApplication.Value), nameof(TravelApplication));
            Require(Enum.IsDefined(args.PurchaseApplication), nameof(PurchaseApplication));
            Require(args.RuleDeference is null || Enum.IsDefined(args.RuleDeference.Value), nameof(RuleDeference));
            Require(args.ChargeKind is null || Enum.IsDefined(args.ChargeKind.Value), nameof(ChargeKind));
            Require(args.AllowanceConcept is null || Enum.IsDefined(args.AllowanceConcept.Value), nameof(AllowanceConcept));
            Require(args.ChargeKind != BaggageChargeKind.WeightPackage || args.AllowanceConcept == BaggageAllowanceConcept.Weight, nameof(AllowanceConcept));
            Require(args.ChargeKind != BaggageChargeKind.ExtraPiece || args.AllowanceConcept == BaggageAllowanceConcept.Piece, nameof(AllowanceConcept));
            Require(args.ChargeKind != BaggageChargeKind.WeightPackage || args.Weight is not null, nameof(Weight));

            var rule = stored ?? new ProvisionBaggageApplicationRule(idGenerator.NewId(), ancillaryProvisionId);

            return () =>
            {
                rule.FreePieces = args.FreePieces;
                rule.FirstExcessPiece = args.FirstExcessPiece;
                rule.LastExcessPiece = args.LastExcessPiece;
                rule.Weight = args.Weight;
                rule.WeightUnit = args.WeightUnit;
                rule.TravelApplication = args.TravelApplication;
                rule.PurchaseApplication = args.PurchaseApplication;
                rule.RuleDeference = args.RuleDeference;
                rule.ChargeKind = args.ChargeKind;
                rule.AllowanceConcept = args.AllowanceConcept;

                return rule;
            };
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionBaggageApplicationRule)}.{field}");
        }
    }
}
