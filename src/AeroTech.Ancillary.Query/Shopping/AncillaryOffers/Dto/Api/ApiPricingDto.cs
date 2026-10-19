namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiPricingDto(
        string Origin,
        string Status,
        string? Unit,
        string? CurrencyCode,
        string? Base,
        string? Total,
        IReadOnlyList<ApiTaxDto> Taxes,
        IReadOnlyList<ApiFeeDto> Fees,
        string? RequestedTotal,
        bool TotalComplete);
}
