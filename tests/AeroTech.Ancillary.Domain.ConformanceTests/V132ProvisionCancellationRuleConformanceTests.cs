using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V132ProvisionCancellationRuleConformanceTests
{
    private const int Irr = 70;

    private static readonly IReadOnlyDictionary<int, int> Currencies = new Dictionary<int, int> { [Eur] = 2, [Irr] = 0 };

    private static ProvisionCancellationRuleArgs Fixed(string reference = "VOL-24H", int? from = 1440, int? to = null, decimal amount = 10m, int currency = Eur)
        => new(reference, CancellationReasonKind.Voluntary, from, to, PenaltyKind.FixedAmount, amount, null, currency, null, true, "CARRIER-TERMS");

    private static ProvisionCancellationRuleArgs Percent(string reference = "VOL-LATE", int? from = 0, int? to = 1440, decimal percentage = 50m)
        => new(reference, CancellationReasonKind.Voluntary, from, to, PenaltyKind.Percentage, null, percentage, null, CancellationPercentageBase.ServiceTotal, false, "CARRIER-TERMS");

    private static ProvisionCancellationRuleArgs Manual(string reference = "NOSHOW", CancellationReasonKind reason = CancellationReasonKind.NoShow)
        => new(reference, reason, null, null, PenaltyKind.ManualReview, null, null, null, null, false, null);

    private static AncillaryProvision Define(
        IReadOnlyList<ProvisionCancellationRuleArgs>? rules,
        AncillaryServiceDefinition? definition = null,
        CommercialDisposition disposition = CommercialDisposition.Paid,
        ProvisionApplicationType applicationType = ProvisionApplicationType.Standard,
        int maxQuantity = 1,
        ProvisionRulesArgs? groups = null,
        SequentialIdGenerator? ids = null)
        => AncillaryProvision.Define(
            5001,
            definition ?? CarrierDefinition(),
            10,
            ServiceCoverageScope.Sector,
            PurchaseStage.Both,
            V122Fixtures.Origin(disposition),
            null,
            QuantityRule.Create(AncillaryQuantityUnit.Each, 1, maxQuantity),
            applicationType,
            CommercialOutcome.Create(disposition, disposition == CommercialDisposition.Paid, false),
            SettlementDefinition.Create(ReissueRefundPolicy.Refundable, FormOfRefund.OriginalPayment, false, false),
            AvailabilityDefinition.Create(false),
            FulfillmentDefinition.Create("Ancillary"),
            Rules(groups, applicationType) with { CancellationRules = rules },
            Currencies,
            ids ?? new SequentialIdGenerator(),
            Now);

    private static void Change(AncillaryProvision provision, IReadOnlyList<ProvisionCancellationRuleArgs>? rules, SequentialIdGenerator ids)
        => provision.Change(
            CarrierDefinition(),
            provision.Sequence,
            provision.CoverageScope,
            provision.PurchaseStage,
            provision.PriceOrigin,
            provision.QuoteProviderKey,
            provision.Quantity,
            provision.ApplicationType,
            provision.Outcome,
            provision.Settlement,
            provision.Availability,
            provision.Fulfillment,
            Rules(null, provision.ApplicationType) with { CancellationRules = rules },
            Currencies,
            ids);

    [Fact]
    public void S1_CR01_a_provision_is_defined_without_cancellation_rules_and_with_filed_rules_of_every_penalty_kind()
    {
        Assert.Empty(Define(null).CancellationRules);
        Assert.Empty(Define([]).CancellationRules);

        var provision = Define([Fixed(), Percent(), Manual(), Manual("SUPPLIER", CancellationReasonKind.SupplierFailure)]);

        Assert.Equal(
            new[]
            {
                ("VOL-24H", PenaltyKind.FixedAmount, (decimal?)10m, (decimal?)null, (int?)Eur, (CancellationPercentageBase?)null),
                ("VOL-LATE", PenaltyKind.Percentage, null, 50m, null, CancellationPercentageBase.ServiceTotal),
                ("NOSHOW", PenaltyKind.ManualReview, null, null, null, null),
                ("SUPPLIER", PenaltyKind.ManualReview, null, null, null, null)
            },
            provision.CancellationRules.Select(rule => (rule.RuleRef, rule.PenaltyKind, rule.PenaltyFixedAmount, rule.PenaltyPercentage, rule.PenaltyCurrencyId, rule.PercentageBase)));
        Assert.All(provision.CancellationRules, rule => Assert.Equal(provision.Id, rule.AncillaryProvisionId));
        Assert.Equal(10m, Define([Fixed(amount: 10.000m, currency: Irr)]).CancellationRules.Single().PenaltyFixedAmount);
    }

    [Theory]
    [InlineData("kind fields", "PenaltyPercentage")]
    [InlineData("percentage zero", "PenaltyPercentage")]
    [InlineData("percentage above hundred", "PenaltyPercentage")]
    [InlineData("percentage scale", "PenaltyPercentage")]
    [InlineData("percentage without base", "PercentageBase")]
    [InlineData("fixed without currency", "PenaltyCurrencyId")]
    [InlineData("unknown currency", "PenaltyCurrencyId")]
    [InlineData("amount finer than the currency", "PenaltyCurrencyId")]
    [InlineData("negative amount", "PenaltyFixedAmount")]
    [InlineData("numeric penalty without source", "SourceReference")]
    [InlineData("none without source", "SourceReference")]
    [InlineData("window not ascending", "BeforeStartMinutesTo")]
    [InlineData("negative window", "BeforeStartMinutesFrom")]
    [InlineData("empty window end", "BeforeStartMinutesTo")]
    [InlineData("blank reference", "RuleRef")]
    [InlineData("padded reference", "RuleRef")]
    public void S1_CR02_a_rule_that_cannot_be_quoted_as_filed_is_refused_when_it_is_authored(string case_, string field)
    {
        ProvisionCancellationRuleArgs rule = case_ switch
        {
            "kind fields" => Fixed() with { PenaltyPercentage = 5m },
            "percentage zero" => Percent(percentage: 0m),
            "percentage above hundred" => Percent(percentage: 100.5m),
            "percentage scale" => Percent(percentage: 12.34567m),
            "percentage without base" => Percent() with { PercentageBase = null },
            "fixed without currency" => Fixed() with { PenaltyCurrencyId = null },
            "unknown currency" => Fixed(currency: 155),
            "amount finer than the currency" => Fixed(amount: 10.005m),
            "negative amount" => Fixed(amount: -1m),
            "numeric penalty without source" => Fixed() with { SourceReference = null },
            "none without source" => Manual() with { PenaltyKind = PenaltyKind.None },
            "window not ascending" => Fixed(from: 1440, to: 1440),
            "negative window" => Fixed(from: -1),
            "empty window end" => Fixed(from: null, to: 0),
            "blank reference" => Fixed(reference: " "),
            _ => Fixed(reference: " VOL ")
        };

        var failure = BusinessAssert.Throws(16302, 422, () => Define([rule]));

        Assert.Contains($"ProvisionCancellationRule.{field}", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void S1_CR03_rules_of_one_reason_may_not_overlap_in_time_and_a_reference_is_used_once()
    {
        Assert.Equal(2, Define([Fixed(from: 1440, to: null), Percent(from: 0, to: 1440)]).CancellationRules.Count);
        Assert.Equal(2, Define([Fixed(from: null, to: null), Manual("INVOL", CancellationReasonKind.Involuntary)]).CancellationRules.Count);

        var overlap = BusinessAssert.Throws(16302, 422, () => Define([Fixed(from: 1000, to: null), Percent(from: 0, to: 1440)]));
        var openEnded = BusinessAssert.Throws(16302, 422, () => Define([Fixed(from: null, to: null), Percent()]));
        var twice = BusinessAssert.Throws(16302, 422, () => Define([Fixed(), Manual("VOL-24H")]));

        Assert.All(new[] { overlap, openEnded }, failure => Assert.Contains("ProvisionCancellationRule.BeforeStartMinutesFrom", failure.Message, StringComparison.Ordinal));
        Assert.Contains("ProvisionCancellationRule.RuleRef", twice.Message, StringComparison.Ordinal);
        BusinessAssert.Throws(16302, 422, () => Define([Fixed(), null!]));
    }

    [Fact]
    public void S1_CR04_a_change_without_rules_keeps_them_an_empty_list_clears_them_and_a_list_replaces_them()
    {
        var ids = new SequentialIdGenerator();
        var provision = Define([Fixed(), Percent()], ids: ids);
        var kept = provision.CancellationRules.Select(rule => rule.Id).ToList();

        Change(provision, null, ids);

        Assert.Equal(kept, provision.CancellationRules.Select(rule => rule.Id));

        Change(provision, [Fixed(), Manual()], ids);

        Assert.Equal(new[] { "VOL-24H", "NOSHOW" }, provision.CancellationRules.Select(rule => rule.RuleRef));
        Assert.Equal(kept[0], provision.CancellationRules.First().Id);
        Assert.DoesNotContain(kept[1], provision.CancellationRules.Select(rule => rule.Id));

        Change(provision, [], ids);

        Assert.Empty(provision.CancellationRules);
        BusinessAssert.Throws(16302, 422, () => Change(provision, [Fixed(), Fixed()], ids));
        Assert.Empty(provision.CancellationRules);
    }

    [Fact]
    public void S1_CR05_the_profile_limits_published_in_the_form_are_enforced_when_the_provision_is_authored()
    {
        var freeMeal = CarrierDefinition(variant: AncillaryVariant.FreeSpecialMeal);
        var seat = CarrierDefinition(variant: AncillaryVariant.StandardSeat, pricingUnit: PricingUnit.PerSeat);
        var lounge = CarrierDefinition(variant: AncillaryVariant.Lounge, serviceDateBasis: ServiceDateBasis.ServiceStart);
        var seatRules = new ProvisionRulesArgs(null, null, null, null, null, null, null, null, null, new ProvisionSeatApplicationArgs([], ["W"]));
        var flightRules = new ProvisionRulesArgs(null, null, null, new ProvisionFlightApplicationArgs([], [], ["1124"], [], []), null, null, null, null, null, null);

        Assert.Equal(
            new[] { CommercialDisposition.Free, CommercialDisposition.NotAvailable },
            AncillaryProvision.AllowedDispositions(freeMeal));
        Assert.Equal(new[] { PriceOrigin.Free, PriceOrigin.NotAvailable }, AncillaryProvision.AllowedPriceOrigins(freeMeal));
        Assert.Equal(new[] { PurchaseStage.PreOrder, PurchaseStage.PostTicketed, PurchaseStage.Both }, AncillaryProvision.AllowedPurchaseStages(freeMeal));
        Assert.Equal((AncillaryQuantityUnit?)AncillaryQuantityUnit.Each, AncillaryProvision.QuantityUnitOf(seat));
        Assert.Equal((int?)1, AncillaryProvision.MaxQuantityOf(seat));
        Assert.Null(AncillaryProvision.MaxQuantityOf(freeMeal));

        Assert.Equal(CommercialDisposition.Free, Define(null, freeMeal, CommercialDisposition.Free).Outcome.Disposition);
        Assert.Equal(CommercialDisposition.NotAvailable, Define(null, freeMeal, CommercialDisposition.NotAvailable).Outcome.Disposition);
        BusinessAssert.Throws(16321, 409, () => Define(null, freeMeal, CommercialDisposition.Paid));
        BusinessAssert.Throws(16321, 409, () => Define(null, seat, applicationType: ProvisionApplicationType.Seat, maxQuantity: 2, groups: seatRules));
        Assert.Equal(1, Define(null, seat, applicationType: ProvisionApplicationType.Seat, groups: seatRules).Quantity.MaxQuantity);
        BusinessAssert.Throws(16313, 409, () => Define(null, lounge, groups: flightRules));
        Assert.Null(Define(null, lounge).FlightApplication);
    }
}
