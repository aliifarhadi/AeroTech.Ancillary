using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments
{
    public sealed record AncillaryPricingLineArgs(
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
