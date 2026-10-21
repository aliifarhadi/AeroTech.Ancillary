namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelSelectionLineDto(
        string ClientSelectionId,
        string Status,
        IReadOnlyList<string> ReasonCodes,
        bool CanPurchase,
        string? PricedSelectionRef,
        DateTimeOffset? ValidUntilUtc,
        OtaPanelSelectionResultDto? Ancillary);
}
