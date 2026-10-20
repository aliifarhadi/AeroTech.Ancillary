using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2OptionRefTests
{
    private const string Adult = "ADT-1";
    private const string Child = "CHD-1";

    private static async Task<(ShoppingLab Lab, AncillaryShoppingSnapshot Shopping)> ShopAsync(string code)
    {
        var lab = new ShoppingLab();
        var definition = lab.Definition(code);
        var provision = lab.Provision(definition);

        if (provision.PriceOrigin == PriceOrigin.Filed)
            lab.Price(definition, provision, Rate(30m));

        return (lab, await SnapshotAsync(lab));
    }

    private static async Task<SelectionLineSnapshot> EvaluateAsync(ShoppingLab lab, string offerItemRef, AncillarySelection selection, IReadOnlyDictionary<string, string>? optionRefs)
    {
        var (service, _) = await SelectionDesk.BatchAsync(lab);

        return (await service.ExecuteAsync(new OfferQuery(OfferWire.OfferId), [new SelectionLineRequest("line", SelectionLineKind.Ancillary, offerItemRef, selection, null, null, optionRefs)], Sale)).Lines.Single();
    }

    private static AncillaryOfferItem Item(AncillaryShoppingSnapshot shopping, string code, string traveller)
        => shopping.Items.First(item => item.Candidate.VariantCode == code && item.Candidate.Travellers.SequenceEqual([traveller]));

    [Fact]
    public async Task B6_an_issued_option_reference_for_an_authored_plan_or_equipment_kind_is_accepted()
    {
        var (wifiLab, wifi) = await ShopAsync("A24");
        var (equipmentLab, equipment) = await ShopAsync("A06");
        var plan = Item(wifi, "A24", Adult);
        var bike = Item(equipment, "A06", Adult);

        var planLine = await EvaluateAsync(
            wifiLab,
            plan.OfferItemRef,
            new AncillarySelection { PlanCode = "WIFI_FULL_FLIGHT", DeviceCount = 1 },
            new Dictionary<string, string> { ["planCode"] = OfferItemRef.OptionOf(plan.OfferItemRef, "planCode", "WIFI_FULL_FLIGHT") });
        var bikeLine = await EvaluateAsync(
            equipmentLab,
            bike.OfferItemRef,
            new AncillarySelection { EquipmentKind = "BIKE", Dimensions = new SelectedDimensions(190m, 70m, 30m), WeightKg = 20m },
            new Dictionary<string, string> { ["equipmentKind"] = OfferItemRef.OptionOf(bike.OfferItemRef, "equipmentKind", "BIKE") });

        Assert.Equal((SelectionEvaluationStatus.Accepted, true), (planLine.Status, planLine.CanPurchase));
        Assert.Equal((SelectionEvaluationStatus.Accepted, true), (bikeLine.Status, bikeLine.CanPurchase));
    }

    [Theory]
    [InlineData("wrong-ref")]
    [InlineData("other-traveller-ref")]
    [InlineData("key-case")]
    [InlineData("value-not-selected")]
    public async Task B6_an_option_reference_that_was_not_issued_for_this_candidate_field_and_value_rejects_the_line(string shape)
    {
        var (lab, shopping) = await ShopAsync("A24");
        var plan = Item(shopping, "A24", Adult);
        var other = Item(shopping, "A24", Child);
        var issued = OfferItemRef.OptionOf(plan.OfferItemRef, "planCode", "WIFI_FULL_FLIGHT");
        var optionRefs = shape switch
        {
            "wrong-ref" => new Dictionary<string, string> { ["planCode"] = issued[..^2] + "xx" },
            "other-traveller-ref" => new Dictionary<string, string> { ["planCode"] = OfferItemRef.OptionOf(other.OfferItemRef, "planCode", "WIFI_FULL_FLIGHT") },
            "key-case" => new Dictionary<string, string> { ["PlanCode"] = issued },
            _ => new Dictionary<string, string> { ["planCode"] = issued, ["deviceCount"] = issued }
        };

        var line = await EvaluateAsync(lab, plan.OfferItemRef, new AncillarySelection { PlanCode = "WIFI_FULL_FLIGHT", DeviceCount = 1 }, optionRefs);

        Assert.Equal((SelectionEvaluationStatus.Rejected, false, ShoppingReasonCodes.SelectedOptionNotOffered), (line.Status, line.CanPurchase, line.ReasonCodes.Single()));
    }

    [Fact]
    public async Task B6_typed_measurements_need_no_option_reference()
    {
        var (lab, shopping) = await ShopAsync("A06");
        var bike = Item(shopping, "A06", Adult);

        var line = await EvaluateAsync(lab, bike.OfferItemRef, new AncillarySelection { EquipmentKind = "BIKE", Dimensions = new SelectedDimensions(190m, 70m, 30m), WeightKg = 20m }, null);

        Assert.Equal((SelectionEvaluationStatus.Accepted, true), (line.Status, line.CanPurchase));
    }

    [Fact]
    public async Task B6_a_dynamic_quote_without_a_trusted_option_source_refuses_a_guessed_reference_and_never_sells_a_guessed_value()
    {
        var (lab, shopping) = await ShopAsync("A10");
        var upgrade = Item(shopping, "A10", Adult);
        var guessed = new AncillarySelection { TargetCabinId = 2, QuoteRef = "Q-1" };

        var withRef = await EvaluateAsync(lab, upgrade.OfferItemRef, guessed, new Dictionary<string, string> { ["quoteRef"] = OfferItemRef.OptionOf(upgrade.OfferItemRef, "quoteRef", "Q-1") });
        var withoutRef = await EvaluateAsync(lab, upgrade.OfferItemRef, guessed, null);

        Assert.Equal((SelectionEvaluationStatus.Rejected, false, ShoppingReasonCodes.SelectedOptionNotOffered), (withRef.Status, withRef.CanPurchase, withRef.ReasonCodes.Single()));
        Assert.False(withoutRef.CanPurchase);
    }

    [Fact]
    public async Task B6_a_seat_line_cannot_carry_option_references()
    {
        var (service, _) = await SelectionDesk.BatchAsync(new ShoppingLab());

        var refused = await Assert.ThrowsAsync<BusinessException>(
            () => service.ExecuteAsync(new OfferQuery(OfferWire.OfferId), [new SelectionLineRequest("seat", SelectionLineKind.SeatMap, null, null, "seat-id", null, new Dictionary<string, string> { ["seatNumber"] = "x" })], Sale));

        Assert.Equal((16801, 422), (refused.Code, refused.HttpStatus));
    }
}
