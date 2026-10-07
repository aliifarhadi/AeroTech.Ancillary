using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionPriceLineArgs(
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        decimal UnitAmount,
        int? CountryId = null,
        int? StationAirportId = null);
}
