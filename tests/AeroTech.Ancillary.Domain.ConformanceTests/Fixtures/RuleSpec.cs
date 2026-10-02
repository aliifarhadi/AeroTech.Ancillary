using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.ValueObjects;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public sealed record RuleSpec
{
    public const int Currency = 978;

    public static RuleSpec R => new();

    public static RuleSpec RG => new() { ProductRef = "XBAGG", Lines = [Ancillary(30.00m)] };

    public static RuleSpec RL => new() { ProductRef = "LNGTHR", Lines = [Ancillary(20.00m)] };

    public static RuleSpec RLG => new() { ProductRef = "LNGXLG", Lines = [Ancillary(15.00m)] };

    public int OwnerAirlineId { get; init; } = SubCodes.AirlineId;

    public string ProductRef { get; init; } = "XBAG1";

    public int Priority { get; init; } = 1;

    public int CurrencyId { get; init; } = Currency;

    public IReadOnlyList<PriceLineArgs> Lines { get; init; } = [Ancillary(35.00m), Tax("VAT", 3.50m, "Value added tax")];

    public DateTimeOffset? SalesFrom { get; init; }

    public DateTimeOffset? SalesTo { get; init; }

    public DateOnly? TravelFrom { get; init; }

    public DateOnly? TravelTo { get; init; }

    public IReadOnlyList<PassengerTypeCode>? PassengerTypes { get; init; }

    public IReadOnlyList<int>? OriginAirportIds { get; init; }

    public IReadOnlyList<int>? DestinationAirportIds { get; init; }

    public static PriceLineArgs Ancillary(decimal amount, string? name = null)
        => new(AncillaryPriceLineCategory.Ancillary, null, name, amount);

    public static PriceLineArgs Tax(string? code, decimal amount, string? name = null)
        => new(AncillaryPriceLineCategory.Tax, code, name, amount);

    public AncillaryPriceRule Define(IIdGenerator ids, DateTimeOffset createdAt)
        => AncillaryPriceRule.Define(
            ids.NewId(),
            OwnerAirlineId,
            ProductRef,
            Priority,
            CurrencyId,
            Lines,
            SalesFrom,
            SalesTo,
            TravelFrom,
            TravelTo,
            new PriceRuleConditions(PassengerTypes, OriginAirportIds, DestinationAirportIds),
            ids,
            createdAt);

    public void Change(AncillaryPriceRule rule, IIdGenerator ids)
        => rule.Change(
            Priority,
            CurrencyId,
            Lines,
            SalesFrom,
            SalesTo,
            TravelFrom,
            TravelTo,
            new PriceRuleConditions(PassengerTypes, OriginAirportIds, DestinationAirportIds),
            ids);
}
