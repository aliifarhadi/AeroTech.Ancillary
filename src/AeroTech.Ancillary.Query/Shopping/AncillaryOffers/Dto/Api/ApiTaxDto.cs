namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiTaxDto(string Amount, string? Code, string? CurrencyCode, bool Included);
}
