using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    public sealed record PricingLineInput(
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
