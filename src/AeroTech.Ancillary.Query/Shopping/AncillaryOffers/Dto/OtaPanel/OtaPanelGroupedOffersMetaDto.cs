namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedOffersMetaDto(
        string? OfferId,
        long? OrderId,
        int CurrencyId,
        DateTimeOffset EvaluatedAtUtc,
        string ShoppingStage);
}
