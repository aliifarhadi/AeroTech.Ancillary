using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionCancellationRule : Entity<long>
    {
        private const int RuleRefMaxLength = 50;
        private const int SourceReferenceMaxLength = 256;
        private const decimal MaximumPercentage = 100m;
        private const int PercentageScale = 4;

        private ProvisionCancellationRule()
        {
        }

        internal ProvisionCancellationRule(
            long id,
            long ancillaryProvisionId,
            ProvisionCancellationRuleArgs args,
            IReadOnlyDictionary<int, int> currencyDecimalPlaces)
        {
            Ensure(args, currencyDecimalPlaces);

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            RuleRef = args.RuleRef;
            ReasonKind = args.ReasonKind;
            BeforeStartMinutesFrom = args.BeforeStartMinutesFrom;
            BeforeStartMinutesTo = args.BeforeStartMinutesTo;
            PenaltyKind = args.PenaltyKind;
            PenaltyFixedAmount = args.PenaltyFixedAmount;
            PenaltyPercentage = args.PenaltyPercentage;
            PenaltyCurrencyId = args.PenaltyCurrencyId;
            PercentageBase = args.PercentageBase;
            WaiverPermitted = args.WaiverPermitted;
            SourceReference = args.SourceReference;
        }

        public long AncillaryProvisionId { get; private set; }

        public string RuleRef { get; private set; } = default!;

        public CancellationReasonKind ReasonKind { get; private set; }

        public int? BeforeStartMinutesFrom { get; private set; }

        public int? BeforeStartMinutesTo { get; private set; }

        public PenaltyKind PenaltyKind { get; private set; }

        public decimal? PenaltyFixedAmount { get; private set; }

        public decimal? PenaltyPercentage { get; private set; }

        public int? PenaltyCurrencyId { get; private set; }

        public CancellationPercentageBase? PercentageBase { get; private set; }

        public bool WaiverPermitted { get; private set; }

        public string? SourceReference { get; private set; }

        internal bool SameAs(ProvisionCancellationRuleArgs args)
            => RuleRef == args.RuleRef
               && ReasonKind == args.ReasonKind
               && BeforeStartMinutesFrom == args.BeforeStartMinutesFrom
               && BeforeStartMinutesTo == args.BeforeStartMinutesTo
               && PenaltyKind == args.PenaltyKind
               && PenaltyFixedAmount == args.PenaltyFixedAmount
               && PenaltyPercentage == args.PenaltyPercentage
               && PenaltyCurrencyId == args.PenaltyCurrencyId
               && PercentageBase == args.PercentageBase
               && WaiverPermitted == args.WaiverPermitted
               && SourceReference == args.SourceReference;

        internal static bool Overlap(ProvisionCancellationRuleArgs first, ProvisionCancellationRuleArgs second)
            => first.ReasonKind == second.ReasonKind
               && (first.BeforeStartMinutesFrom ?? 0) < (second.BeforeStartMinutesTo ?? int.MaxValue)
               && (second.BeforeStartMinutesFrom ?? 0) < (first.BeforeStartMinutesTo ?? int.MaxValue);

        private static void Ensure(ProvisionCancellationRuleArgs args, IReadOnlyDictionary<int, int> currencyDecimalPlaces)
        {
            Require(!string.IsNullOrWhiteSpace(args.RuleRef) && args.RuleRef.Length <= RuleRefMaxLength && args.RuleRef == args.RuleRef.Trim(), nameof(RuleRef));
            Require(Enum.IsDefined(args.ReasonKind), nameof(ReasonKind));
            Require(Enum.IsDefined(args.PenaltyKind), nameof(PenaltyKind));
            Require(args.BeforeStartMinutesFrom is null or >= 0, nameof(BeforeStartMinutesFrom));
            Require(args.BeforeStartMinutesTo is null or > 0, nameof(BeforeStartMinutesTo));
            Require(
                args.BeforeStartMinutesFrom is not { } from || args.BeforeStartMinutesTo is not { } to || from < to,
                nameof(BeforeStartMinutesTo));
            Require(args.SourceReference is null || (args.SourceReference.Length <= SourceReferenceMaxLength && !string.IsNullOrWhiteSpace(args.SourceReference)), nameof(SourceReference));
            Require(
                args.PenaltyKind is PenaltyKind.ProviderQuoted or PenaltyKind.ManualReview || args.SourceReference is not null,
                nameof(SourceReference));

            var fixedAmount = args.PenaltyKind == PenaltyKind.FixedAmount;
            var percentage = args.PenaltyKind == PenaltyKind.Percentage;

            Require(fixedAmount ? args.PenaltyFixedAmount is >= 0 : args.PenaltyFixedAmount is null, nameof(PenaltyFixedAmount));
            Require(fixedAmount ? args.PenaltyCurrencyId is > 0 : args.PenaltyCurrencyId is null, nameof(PenaltyCurrencyId));
            Require(
                percentage ? args.PenaltyPercentage is > 0 and <= MaximumPercentage && ScaleOf(args.PenaltyPercentage.Value) <= PercentageScale : args.PenaltyPercentage is null,
                nameof(PenaltyPercentage));
            Require(
                percentage ? args.PercentageBase is { } percentageBase && Enum.IsDefined(percentageBase) : args.PercentageBase is null,
                nameof(PercentageBase));

            if (args is { PenaltyFixedAmount: { } amount, PenaltyCurrencyId: { } currencyId })
                Require(
                    currencyDecimalPlaces.TryGetValue(currencyId, out var decimalPlaces) && decimalPlaces >= 0 && ScaleOf(amount) <= decimalPlaces,
                    nameof(PenaltyCurrencyId));
        }

        private static int ScaleOf(decimal amount)
        {
            var scale = amount.Scale;

            while (scale > 0 && decimal.Round(amount, scale - 1) == amount)
                scale--;

            return scale;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionCancellationRule)}.{field}");
        }
    }
}
