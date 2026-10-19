namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferPriceLineDto(
        string? Code,
        decimal Amount,
        int CurrencyId,
        int? CountryId,
        int? StationAirportId,
        string? ApplicationUnit,
        string? TaxTreatment);
}
