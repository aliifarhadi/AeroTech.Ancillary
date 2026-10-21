namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiSelectionLineDto(
        string ClientSelectionId,
        string Status,
        IReadOnlyList<string> ReasonCodes,
        bool CanPurchase,
        string? PricedSelectionRef,
        DateTimeOffset? ValidUntilUtc,
        ApiSelectionResultDataDto? Ancillary);
}
