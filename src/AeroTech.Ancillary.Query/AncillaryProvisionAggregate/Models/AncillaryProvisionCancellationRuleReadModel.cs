using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionCancellationRuleReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public string RuleRef { get; set; } = default!;

        public CancellationReasonKind ReasonKind { get; set; }

        public int? BeforeStartMinutesFrom { get; set; }

        public int? BeforeStartMinutesTo { get; set; }

        public PenaltyKind PenaltyKind { get; set; }

        public decimal? PenaltyFixedAmount { get; set; }

        public decimal? PenaltyPercentage { get; set; }

        public int? PenaltyCurrencyId { get; set; }

        public CancellationPercentageBase? PercentageBase { get; set; }

        public bool WaiverPermitted { get; set; }

        public string? SourceReference { get; set; }
    }
}
