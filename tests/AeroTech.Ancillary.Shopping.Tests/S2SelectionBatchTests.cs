using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2SelectionBatchTests
{
    private const string Adult = "ADT-1";
    private const string Child = "CHD-1";

    private static readonly AncillarySalesScope Scope = Sale;

    private static ShoppingLab BagLab(params AncillaryPriceComponentArgs[] bagComponents)
    {
        var lab = new ShoppingLab();
        var bag = lab.Definition("A01");

        lab.Price(bag, lab.Provision(bag), Rate(100m, components: bagComponents.Length > 0 ? bagComponents : [Tax("VAT", 10m, TaxTreatment.AddedToBase)]));

        return lab;
    }

    private static async Task<string> BagRefAsync(ShoppingLab lab, string traveller)
        => (await SnapshotAsync(lab)).Items.First(item => item.Candidate.VariantCode == "A01" && item.Candidate.Travellers.SequenceEqual([traveller])).OfferItemRef;

    private static SelectionLineRequest Bag(string id, string offerItemRef, int quantity) => new(id, offerItemRef, new AncillarySelection { Quantity = quantity });

    private static Task<SelectionBatchSnapshot> EvaluateAsync(EvaluateBatchService service, params SelectionLineRequest[] lines)
        => service.ExecuteAsync(new OfferQuery(OfferWire.OfferId), lines, Scope);

    private static (SelectionEvaluationStatus, bool, decimal?, string) Verdict(SelectionLineSnapshot line)
        => (line.Status, line.CanPurchase, line.Total, string.Join(',', line.ReasonCodes));

    [Fact]
    public async Task PRICE_01_two_lines_of_one_offer_are_priced_by_their_own_quantity_and_the_basket_adds_them_without_an_order_bound_reference()
    {
        var lab = BagLab();
        var (service, _) = await SelectionDesk.BatchAsync(lab);

        var batch = await EvaluateAsync(service, Bag("adult", await BagRefAsync(lab, Adult), 1), Bag("child", await BagRefAsync(lab, Child), 2));

        Assert.Equal(new[] { "adult", "child" }, batch.Lines.Select(line => line.ClientSelectionId));
        Assert.Equal((SelectionEvaluationStatus.Accepted, true, (decimal?)110m, ""), Verdict(batch.Lines[0]));
        Assert.Equal((SelectionEvaluationStatus.Accepted, true, (decimal?)220m, ""), Verdict(batch.Lines[1]));
        Assert.All(batch.Lines, line => Assert.Null(line.PricedSelection));
        Assert.Equal((false, (decimal?)330m, Eur), (batch.Basket.AllReady, batch.Basket.Total, batch.Basket.CurrencyId));
        Assert.Equal(new[] { ShoppingReasonCodes.OrderBindingRequired }, batch.Basket.ReasonCodes);
    }

    [Fact]
    public async Task PRICE_19_one_unusable_line_leaves_the_basket_not_ready_while_every_line_keeps_its_own_verdict()
    {
        var lab = BagLab();
        var (service, _) = await SelectionDesk.BatchAsync(lab);

        var batch = await EvaluateAsync(
            service,
            Bag("valid", await BagRefAsync(lab, Adult), 1),
            Bag("unknown", "not-an-offer-item", 1),
            Bag("zero", await BagRefAsync(lab, Child), 0));

        Assert.Equal((SelectionEvaluationStatus.Accepted, true, (decimal?)110m, ""), Verdict(batch.Lines[0]));
        Assert.Equal((SelectionEvaluationStatus.Rejected, false, (decimal?)null, ShoppingReasonCodes.OfferItemNotOffered), Verdict(batch.Lines[1]));
        Assert.Equal((SelectionEvaluationStatus.Rejected, false), (batch.Lines[2].Status, batch.Lines[2].CanPurchase));
        Assert.Equal((false, (decimal?)null), (batch.Basket.AllReady, batch.Basket.Total));
    }

    [Theory]
    [InlineData(1, 1, ShoppingReasonCodes.DuplicateSelection)]
    [InlineData(1, 2, ShoppingReasonCodes.QuantitySplitAcrossLines)]
    public async Task PRICE_20_B4_the_same_offer_item_twice_with_the_same_or_a_split_quantity_rejects_both_lines_without_choosing_a_winner(int first, int second, string reason)
    {
        var lab = BagLab();
        var (service, _) = await SelectionDesk.BatchAsync(lab);
        var bag = await BagRefAsync(lab, Adult);

        var batch = await EvaluateAsync(service, Bag("first", bag, first), Bag("second", bag, second));

        Assert.All(batch.Lines, line => Assert.Equal((SelectionEvaluationStatus.Rejected, false, reason), (line.Status, line.CanPurchase, line.ReasonCodes.Single())));
        Assert.All(batch.Lines, line => Assert.Equal(SelectionEvaluationStatus.Accepted, line.Ancillary!.Evaluation.Status));
        Assert.False(batch.Basket.AllReady);
    }

    [Fact]
    public async Task B4_two_different_selections_of_one_candidate_are_refused_because_no_rule_authorizes_a_repeated_instance()
    {
        var lab = new ShoppingLab();
        var wifi = lab.Definition("A24");

        lab.Price(wifi, lab.Provision(wifi), Rate(30m));

        var (service, _) = await SelectionDesk.BatchAsync(lab);
        var plan = (await SnapshotAsync(lab)).Items.First(item => item.Candidate.VariantCode == "A24").OfferItemRef;

        var batch = await EvaluateAsync(
            service,
            new SelectionLineRequest("one-device", plan, new AncillarySelection { PlanCode = wifi.ServiceDefinitionRef, DeviceCount = 1 }),
            new SelectionLineRequest("two-devices", plan, new AncillarySelection { PlanCode = wifi.ServiceDefinitionRef, DeviceCount = 2 }));

        Assert.All(batch.Lines, line => Assert.Equal((SelectionEvaluationStatus.Rejected, ShoppingReasonCodes.RepeatedInstanceNotAllowed), (line.Status, line.ReasonCodes.Single())));
    }

    [Fact]
    public async Task B4_the_same_product_for_another_traveller_or_another_bound_is_never_a_duplicate_and_one_line_keeps_its_quantity_cap()
    {
        var lab = BagLab();
        var (service, _) = await SelectionDesk.BatchAsync(lab);
        var bags = (await SnapshotAsync(lab)).Items.Where(item => item.Candidate.VariantCode == "A01").Select(item => item.OfferItemRef).ToList();

        var independent = await EvaluateAsync(service, bags.Select((bag, index) => Bag($"bag-{index}", bag, 1)).ToArray());
        var aboveCap = await EvaluateAsync(service, Bag("seven", bags[0], 7));

        Assert.True(bags.Count >= 4);
        Assert.All(independent.Lines, line => Assert.Equal((SelectionEvaluationStatus.Accepted, true), (line.Status, line.CanPurchase)));
        Assert.Equal(SelectionEvaluationStatus.Rejected, aboveCap.Lines[0].Status);
        Assert.Contains(aboveCap.Lines[0].Ancillary!.Evaluation.FieldIssues, issue => issue.ReasonCode == ShoppingReasonCodes.QuantityAboveMaximum);
    }

    [Fact]
    public async Task PRICE_12_a_shared_ticket_fee_is_listed_once_for_the_basket_and_never_charged_inside_a_line()
    {
        var lab = BagLab(Fee("TKT", 7m, FeeApplicationUnit.Ticket));
        var (service, _) = await SelectionDesk.BatchAsync(lab);

        var batch = await EvaluateAsync(service, Bag("adult", await BagRefAsync(lab, Adult), 1), Bag("child", await BagRefAsync(lab, Child), 1));

        Assert.All(batch.Lines, line => Assert.Equal((SelectionEvaluationStatus.Accepted, false, (decimal?)null), (line.Status, line.CanPurchase, line.Total)));
        Assert.Equal(("TKT", 7m, FeeApplicationUnit.Ticket), (batch.Basket.SharedFees.Single().Code, batch.Basket.SharedFees.Single().Amount, batch.Basket.SharedFees.Single().FeeApplicationUnit!.Value));
        Assert.Equal((false, (decimal?)null), (batch.Basket.AllReady, batch.Basket.Total));
        Assert.Contains(ShoppingReasonCodes.SharedFeeNotApplied, batch.Basket.ReasonCodes);
    }

    [Fact]
    public async Task S2_BATCH_each_surface_maps_the_same_canonical_lines_with_its_own_identifiers()
    {
        var lab = BagLab();
        var (service, _) = await SelectionDesk.BatchAsync(lab);
        var batch = await EvaluateAsync(service, Bag("bag", await BagRefAsync(lab, Adult), 1), Bag("child-bag", await BagRefAsync(lab, Child), 1));

        var backoffice = BackofficeSelectionBatchMapper.Map(batch);
        var api = ApiSelectionBatchMapper.Map(batch, new ApiOfferReferences(batch.Shopping.Offer, new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<long, BaggageSpecificationArgs>()));
        var otaPanel = OtaPanelSelectionBatchMapper.Map(batch);

        Assert.Equal(("Accepted", true, (string?)null), (backoffice.Lines[0].Status.Name, backoffice.Lines[0].Ancillary!.CanPurchase, backoffice.Lines[0].Ancillary!.PricedSelectionRef));
        Assert.Equal(new[] { Adult }, backoffice.Lines[0].Ancillary!.Beneficiary.TravellerRefs);
        Assert.Equal((ApiSelectionBatchMapper.AncillarySelectionBatch, "Accepted", "2"), (api.Data.Type, api.Data.Lines[1].Status, api.Data.Lines[1].Ancillary!.Beneficiary.TravellerRefs.Single()));
        Assert.Equal((Child, 110m), (otaPanel.Lines[1].Ancillary!.Beneficiary.TravellerRefs.Single(), otaPanel.Lines[1].Ancillary!.ExactPrice.LineTotal!.Value));
        Assert.Equal((false, (decimal?)220m), (otaPanel.Basket.AllReady, otaPanel.Basket.Total));
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("duplicate-client-id")]
    [InlineData("without-item")]
    [InlineData("without-client-id")]
    public async Task S2_BATCH_a_malformed_basket_is_refused_before_any_source_is_read(string shape)
    {
        var (service, _) = await SelectionDesk.BatchAsync(new ShoppingLab());
        SelectionLineRequest[] lines = shape switch
        {
            "empty" => [],
            "duplicate-client-id" => [Bag("same", "item-a", 1), Bag("same", "item-b", 1)],
            "without-item" => [new SelectionLineRequest("line", null, new AncillarySelection())],
            _ => [new SelectionLineRequest(null, "item", null)]
        };

        var refused = await Assert.ThrowsAsync<BusinessException>(() => EvaluateAsync(service, lines));

        Assert.Equal((16801, 422), (refused.Code, refused.HttpStatus));
    }
}
