using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts
{
    public sealed record AncillaryPriceRuleReadModelSnapshot(
        long AncillaryPriceRuleId,
        int OwnerAirlineId,
        string ProductRef,
        int Priority,
        int CurrencyId,
        DateTimeOffset? SalesFrom,
        DateTimeOffset? SalesTo,
        DateOnly? TravelFrom,
        DateOnly? TravelTo,
        IReadOnlyCollection<PassengerTypeCode>? PassengerTypes,
        IReadOnlyCollection<int>? OriginAirportIds,
        IReadOnlyCollection<int>? DestinationAirportIds,
        AncillaryPriceRuleStatus Status,
        DateTimeOffset CreatedAt)
    {
        public IReadOnlyList<PriceLineSnapshot> Lines { get; init; } = Array.Empty<PriceLineSnapshot>();
    }

    public sealed record PriceLineSnapshot(
        long PriceLineId,
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        decimal Amount);
}
