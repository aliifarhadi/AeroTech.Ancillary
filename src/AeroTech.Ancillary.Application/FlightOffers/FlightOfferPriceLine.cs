using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record FlightOfferPriceLine(
        FlightOfferPriceCategory Category,
        string? Name,
        string? Code,
        string? Reference,
        decimal Amount,
        int CurrencyId,
        bool IsPercentage,
        decimal EquivalentAmount,
        int EquivalentCurrencyId,
        string? RateOfExchangePeriodId);
}
