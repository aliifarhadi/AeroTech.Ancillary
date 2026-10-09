using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments
{
    public sealed record AncillaryPriceComponentArgs(
        AncillaryPriceLineCategory Category,
        string Code,
        string? Name,
        int? CountryId,
        int? StationAirportId,
        decimal Amount,
        int CurrencyId,
        FeeApplicationUnit? FeeApplicationUnit,
        bool? TaxIncludedInSource);
}
