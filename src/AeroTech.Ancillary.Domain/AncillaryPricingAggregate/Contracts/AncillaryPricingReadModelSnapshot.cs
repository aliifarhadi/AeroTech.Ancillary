using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts
{
    public sealed record AncillaryPricingReadModelSnapshot(
        long PricingId,
        long AncillaryProvisionId,
        PricingUnit? PricingUnit,
        int Version,
        int CurrencyId,
        FeeApplicationUnit? FeeApplicationUnit,
        PricingStatus Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt,
        IReadOnlyList<AncillaryPricingLineReadModelSnapshot> PriceLines);

    public sealed record AncillaryPricingLineReadModelSnapshot(
        long PriceLineId,
        PassengerTypeCode? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        int? CountryId,
        int? StationAirportId,
        decimal Amount);
}
