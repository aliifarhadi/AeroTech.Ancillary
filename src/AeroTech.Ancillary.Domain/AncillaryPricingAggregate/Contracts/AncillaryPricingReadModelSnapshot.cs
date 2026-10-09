using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts
{
    public sealed record AncillaryPricingReadModelSnapshot(
        long PricingId,
        long AncillaryProvisionId,
        PricingUnit? PricingUnit,
        int Version,
        PricingStatus Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt,
        IReadOnlyList<AncillaryPricingRateReadModelSnapshot> Rates);

    public sealed record AncillaryPricingRateReadModelSnapshot(
        long RateId,
        PassengerTypeCode? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        int CurrencyId,
        decimal BaseAmount,
        IReadOnlyList<AncillaryPriceComponentReadModelSnapshot> Components);

    public sealed record AncillaryPriceComponentReadModelSnapshot(
        long ComponentId,
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        int? CountryId,
        int? StationAirportId,
        decimal Amount,
        int CurrencyId,
        FeeApplicationUnit? FeeApplicationUnit,
        bool? TaxIncludedInSource,
        TaxTreatment? TaxTreatment);
}
