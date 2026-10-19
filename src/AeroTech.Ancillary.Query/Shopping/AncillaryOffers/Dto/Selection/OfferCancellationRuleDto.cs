namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferCancellationRuleDto(
        string RuleRef,
        string ReasonKind,
        int? WindowFromMinutes,
        int? WindowToMinutes,
        string PenaltyKind,
        decimal? PenaltyFixedAmount,
        decimal? PenaltyPercentage,
        int? PenaltyCurrencyId,
        string? PercentageBase,
        bool WaiverPermitted,
        string? SourceReference);
}
