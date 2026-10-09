using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    public sealed record MoneyInput(decimal Amount, int CurrencyId);

    public sealed record PricingRateInput(
        PassengerTypeCode? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        MoneyInput BasePrice,
        IReadOnlyList<PriceComponentInput>? Components);

    public sealed record PriceComponentInput(
        AncillaryPriceLineCategory Category,
        string Code,
        string? Name,
        int? CountryId,
        int? StationAirportId,
        MoneyInput Amount,
        FeeApplicationUnit? FeeApplicationUnit,
        bool? TaxIncludedInSource,
        TaxTreatment? TaxTreatment = null);
}
