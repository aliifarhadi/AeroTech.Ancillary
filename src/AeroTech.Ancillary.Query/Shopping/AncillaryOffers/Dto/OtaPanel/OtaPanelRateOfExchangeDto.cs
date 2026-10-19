namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelRateOfExchangeDto(string RateOfExchangeTableId, int FromCurrencyId, int ToCurrencyId, decimal Rate);
}
