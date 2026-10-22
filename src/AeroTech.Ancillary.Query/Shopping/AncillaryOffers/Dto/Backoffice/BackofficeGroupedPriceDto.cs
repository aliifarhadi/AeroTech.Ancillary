namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedPriceDto(int? CurrencyId, decimal? UnitTotal, bool IsTotalComplete);
}
