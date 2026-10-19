namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiFeeDto(string Amount, string? Type, string? CurrencyCode, string? AppliesPer, bool OrderLevel);
}
