namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferExactPriceDto(
        string Origin,
        string? PricingUnit,
        int? CurrencyId,
        int? FiledCurrencyId,
        int? Quantity,
        decimal? ChargeableWeightKg,
        decimal? BaseAmount,
        IReadOnlyList<OfferPriceLineDto> AddedTaxes,
        IReadOnlyList<OfferPriceLineDto> IncludedTaxes,
        IReadOnlyList<OfferPriceLineDto> UnitFees,
        IReadOnlyList<OfferPriceLineDto> SharedFees,
        decimal? UnitTotal,
        decimal? LineTotal,
        OfferFxEvidenceDto? Fx,
        bool IsTotalComplete,
        IReadOnlyList<string> ReasonCodes);
}
