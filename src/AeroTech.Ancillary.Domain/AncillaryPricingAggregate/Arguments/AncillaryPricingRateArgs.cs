using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments
{
    public sealed record AncillaryPricingRateArgs(
        PassengerTypeCode? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        decimal BaseAmount,
        int CurrencyId,
        IReadOnlyList<AncillaryPriceComponentArgs> Components);
}
