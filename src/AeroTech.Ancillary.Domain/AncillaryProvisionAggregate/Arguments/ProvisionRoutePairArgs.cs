using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionRoutePairArgs(
        int OriginAirportId,
        int DestinationAirportId,
        RoutePairDirection Direction);
}
