using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionCancellationRuleArgs(
        string RuleRef,
        CancellationReasonKind ReasonKind,
        int? BeforeStartMinutesFrom,
        int? BeforeStartMinutesTo,
        PenaltyKind PenaltyKind,
        decimal? PenaltyFixedAmount,
        decimal? PenaltyPercentage,
        int? PenaltyCurrencyId,
        CancellationPercentageBase? PercentageBase,
        bool WaiverPermitted,
        string? SourceReference);
}
