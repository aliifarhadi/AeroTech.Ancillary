using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record OfferCancellationRule(
        string RuleRef,
        CancellationReasonKind ReasonKind,
        int? WindowFromMinutes,
        int? WindowToMinutes,
        PenaltyKind PenaltyKind,
        decimal? PenaltyFixedAmount,
        decimal? PenaltyPercentage,
        int? PenaltyCurrencyId,
        CancellationPercentageBase? PercentageBase,
        bool WaiverPermitted,
        string? SourceReference);
}
