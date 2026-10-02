using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public sealed class Phase1AncillaryPriceRuleConformanceTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly SequentialIdGenerator _ids = new();

    [Fact]
    public void P1_R01_DefineCreatesADraftWithItsLinesInTheOrderSent()
    {
        var spec = RuleSpec.R with
        {
            SalesFrom = CreatedAt,
            SalesTo = CreatedAt.AddDays(30),
            TravelFrom = new DateOnly(2026, 10, 10),
            TravelTo = new DateOnly(2026, 10, 10),
            PassengerTypes = [PassengerTypeCode.ADT, PassengerTypeCode.CHD],
            OriginAirportIds = [1],
            DestinationAirportIds = [2, 3]
        };

        var rule = spec.Define(_ids, CreatedAt);

        Assert.Equal(AncillaryPriceRuleStatus.Draft, rule.Status);
        Assert.Equal((10, "XBAG1", 1, 978), (rule.OwnerAirlineId, rule.ProductRef, rule.Priority, rule.CurrencyId));
        Assert.Equal((spec.SalesFrom, spec.SalesTo, spec.TravelFrom, spec.TravelTo), (rule.SalesFrom, rule.SalesTo, rule.TravelFrom, rule.TravelTo));
        Assert.Equal([PassengerTypeCode.ADT, PassengerTypeCode.CHD], rule.Conditions.PassengerTypes);
        Assert.Equal([1], rule.Conditions.OriginAirportIds);
        Assert.Equal([2, 3], rule.Conditions.DestinationAirportIds);
        Assert.Equal(CreatedAt, rule.CreatedAt);
        Assert.Equal(
            [
                (AncillaryPriceLineCategory.Ancillary, null, null, 35.00m),
                (AncillaryPriceLineCategory.Tax, "VAT", "Value added tax", 3.50m)
            ],
            Lines(rule));
    }

    [Fact]
    public void P1_R03_ExactlyOneAncillaryLineAndDistinctTaxCodes()
    {
        Invalid(RuleSpec.R with { Lines = [RuleSpec.Tax("VAT", 3.50m)] });
        Invalid(RuleSpec.R with { Lines = [] });
        Invalid(RuleSpec.R with { Lines = [RuleSpec.Ancillary(35m), RuleSpec.Ancillary(5m)] });
        Invalid(RuleSpec.R with { Lines = [RuleSpec.Ancillary(35m), RuleSpec.Tax(null, 3.50m)] });
        Invalid(RuleSpec.R with { Lines = [RuleSpec.Ancillary(35m), RuleSpec.Tax("", 3.50m)] });
        Invalid(RuleSpec.R with { Lines = [RuleSpec.Ancillary(35m), RuleSpec.Tax("VAT", 3.50m), RuleSpec.Tax("VAT", 1m)] });

        var single = (RuleSpec.R with { Lines = [RuleSpec.Ancillary(35m)] }).Define(_ids, CreatedAt);
        var twoTaxes = (RuleSpec.R with { Lines = [RuleSpec.Ancillary(35m), RuleSpec.Tax("VAT", 3.50m), RuleSpec.Tax("YQ", 1m)] }).Define(_ids, CreatedAt);

        Assert.Single(single.Lines);
        Assert.Equal(3, twoTaxes.Lines.Count);
    }

    [Fact]
    public void P1_R04_AmountsPriorityWindowsAndConditionsAreChecked()
    {
        var instant = CreatedAt;

        Invalid(RuleSpec.R with { Lines = [RuleSpec.Ancillary(0m)] });
        Invalid(RuleSpec.R with { Lines = [RuleSpec.Ancillary(-1m)] });
        Invalid(RuleSpec.R with { Lines = [RuleSpec.Ancillary(35m), RuleSpec.Tax("VAT", 0m)] });
        Invalid(RuleSpec.R with { Lines = [RuleSpec.Ancillary(35m), RuleSpec.Tax("VAT", -3.50m)] });
        Invalid(RuleSpec.R with { Lines = [RuleSpec.Ancillary(35.005m)] });
        Invalid(RuleSpec.R with { Priority = 0 });
        Invalid(RuleSpec.R with { SalesFrom = instant, SalesTo = instant });
        Invalid(RuleSpec.R with { SalesFrom = instant.AddSeconds(1), SalesTo = instant });
        Invalid(RuleSpec.R with { TravelFrom = new DateOnly(2026, 10, 11), TravelTo = new DateOnly(2026, 10, 10) });
        Invalid(RuleSpec.R with { PassengerTypes = [] });
        Invalid(RuleSpec.R with { OriginAirportIds = [] });
        Invalid(RuleSpec.R with { DestinationAirportIds = [] });
        Invalid(RuleSpec.R with { PassengerTypes = [PassengerTypeCode.ADT, PassengerTypeCode.ADT] });
        Invalid(RuleSpec.R with { OriginAirportIds = [1, 1] });
        Invalid(RuleSpec.R with { DestinationAirportIds = [2, 3, 2] });

        var sameDay = (RuleSpec.R with { TravelFrom = new DateOnly(2026, 10, 10), TravelTo = new DateOnly(2026, 10, 10) }).Define(_ids, CreatedAt);

        Assert.Equal(sameDay.TravelFrom, sameDay.TravelTo);
    }

    [Fact]
    public void P1_R05_ChangeReplacesADraftAndIsRefusedAfterwards()
    {
        var rule = RuleSpec.R.Define(_ids, CreatedAt);
        var change = RuleSpec.R with
        {
            Priority = 4,
            CurrencyId = 840,
            Lines = [RuleSpec.Ancillary(40m, "Bag fee")],
            TravelFrom = new DateOnly(2026, 11, 1),
            PassengerTypes = [PassengerTypeCode.CHD]
        };

        change.Change(rule, _ids);

        Assert.Equal((10, "XBAG1", AncillaryPriceRuleStatus.Draft, CreatedAt), (rule.OwnerAirlineId, rule.ProductRef, rule.Status, rule.CreatedAt));
        Assert.Equal((4, 840, new DateOnly(2026, 11, 1)), (rule.Priority, rule.CurrencyId, rule.TravelFrom));
        Assert.Equal([PassengerTypeCode.CHD], rule.Conditions.PassengerTypes);
        Assert.Equal([(AncillaryPriceLineCategory.Ancillary, null, "Bag fee", 40m)], Lines(rule));

        foreach (var status in new[] { AncillaryPriceRuleStatus.Active, AncillaryPriceRuleStatus.Suspended, AncillaryPriceRuleStatus.Retired })
        {
            var published = RuleIn(status);

            BusinessAssert.Throws(16202, 409, () => change.Change(published, _ids));
            Assert.Equal(1, published.Priority);
        }
    }

    [Fact]
    public void P1_R08_StatusChangesOutsideTheLifecycleAreRefused()
    {
        var retired = RuleIn(AncillaryPriceRuleStatus.Retired);

        BusinessAssert.Throws(16203, 409, RuleIn(AncillaryPriceRuleStatus.Draft).Suspend);
        BusinessAssert.Throws(16203, 409, RuleIn(AncillaryPriceRuleStatus.Active).Activate);
        BusinessAssert.Throws(16203, 409, retired.Activate);
        BusinessAssert.Throws(16203, 409, retired.Suspend);
        BusinessAssert.Throws(16203, 409, retired.Retire);

        foreach (var status in new[] { AncillaryPriceRuleStatus.Draft, AncillaryPriceRuleStatus.Active, AncillaryPriceRuleStatus.Suspended })
        {
            var rule = RuleIn(status);

            rule.Retire();

            Assert.Equal(AncillaryPriceRuleStatus.Retired, rule.Status);
        }

        var suspended = RuleIn(AncillaryPriceRuleStatus.Suspended);

        suspended.Activate();

        Assert.Equal(AncillaryPriceRuleStatus.Active, suspended.Status);
    }

    private void Invalid(RuleSpec spec) => BusinessAssert.Throws(16206, 422, () => spec.Define(_ids, CreatedAt));

    private AncillaryPriceRule RuleIn(AncillaryPriceRuleStatus status)
    {
        var rule = RuleSpec.R.Define(_ids, CreatedAt);

        if (status is AncillaryPriceRuleStatus.Active or AncillaryPriceRuleStatus.Suspended)
            rule.Activate();

        if (status == AncillaryPriceRuleStatus.Suspended)
            rule.Suspend();

        if (status == AncillaryPriceRuleStatus.Retired)
            rule.Retire();

        return rule;
    }

    private static (AncillaryPriceLineCategory Category, string? Code, string? Name, decimal Amount)[] Lines(AncillaryPriceRule rule)
        => rule.Lines
            .OrderBy(line => line.Id)
            .Select(line => (line.Category, line.Code, line.Name, line.Amount))
            .ToArray();
}
