using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto
{
    public sealed record BackofficeAncillaryPriceRuleDto(
        long Id,
        int OwnerAirlineId,
        string ProductRef,
        int Priority,
        int CurrencyId,
        IReadOnlyList<PriceLineDto> Lines,
        DateTimeOffset? SalesFrom,
        DateTimeOffset? SalesTo,
        DateOnly? TravelFrom,
        DateOnly? TravelTo,
        PriceRuleConditionsDto Conditions,
        EnumValueDto Status,
        DateTimeOffset CreatedAt);

    public sealed record PriceLineDto(
        EnumValueDto Category,
        string? Code,
        string? Name,
        decimal Amount);

    public sealed record PriceRuleConditionsDto(
        IReadOnlyList<EnumValueDto>? PassengerTypes,
        IReadOnlyList<int>? OriginAirportIds,
        IReadOnlyList<int>? DestinationAirportIds);
}
