using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2FinalShoppingTests
{
    [Theory]
    [InlineData("A01")]
    [InlineData("A12")]
    [InlineData("A13")]
    [InlineData("A23")]
    public async Task S2_ORD_01_order_coverage_is_refused_for_every_variant_except_a22_and_a24(string code)
    {
        var lab = new ShoppingLab();
        var definition = lab.Definition(code);
        var provision = lab.Provision(definition, scope: ServiceCoverageScope.Order);

        if (provision.PriceOrigin == PriceOrigin.Filed)
            lab.Price(definition, provision, Rate(10m));

        var context = Trip.Context(travellers: [Trip.Adult("T1"), Trip.Child("T2")]);
        var result = await lab.Engine.ShopAsync(context, new ShoppingFilter { IncludeNonSellable = true });
        var forged = new CandidateIdentity(definition.Id, provision.Id, ServiceCoverageScope.Order, "", CoveragePlannerOrderRef, null);
        var refused = await Assert.ThrowsAsync<BusinessException>(() => lab.Engine.EvaluateSelectionAsync(context, forged, new AncillarySelection { Quantity = 1 }));

        Assert.DoesNotContain(result.Candidates, candidate => candidate.CandidateIdentity.CoverageScope == ServiceCoverageScope.Order);
        Assert.Equal((16802, 404), (refused.Code, refused.HttpStatus));
    }

    [Fact]
    public async Task S2_W08_a_coupon_that_is_not_upgradable_refuses_the_upgrade_and_an_upgradable_one_still_needs_a_quote()
    {
        var lab = new ShoppingLab();
        var upgrade = lab.Definition("A10");

        lab.Provision(upgrade);

        var blocked = Trip.Context();
        var allowed = Trip.Context();

        blocked = blocked with { TravellerFareFacts = blocked.TravellerFareFacts.Select(fact => fact with { CouponUpgradable = false }).ToList() };
        allowed = allowed with { TravellerFareFacts = allowed.TravellerFareFacts.Select(fact => fact with { CouponUpgradable = true }).ToList() };

        var refused = await lab.Engine.ShopAsync(blocked, new ShoppingFilter { IncludeNonSellable = true });
        var offered = await lab.Engine.ShopAsync(allowed);

        Assert.All(refused.Candidates.Where(candidate => candidate.VariantCode == "A10"), candidate =>
        {
            Assert.Equal(OfferReadiness.NotEligible, candidate.OfferReadiness);
            Assert.Contains(ShoppingReasonCodes.CouponNotUpgradable, candidate.ReasonCodes);
        });
        Assert.DoesNotContain(offered.Candidates.Single(candidate => candidate.VariantCode == "A10").ReasonCodes, code => code == ShoppingReasonCodes.CouponNotUpgradable);
        Assert.NotEqual(OfferReadiness.Selectable, offered.Candidates.Single(candidate => candidate.VariantCode == "A10").OfferReadiness);
    }

    [Theory]
    [InlineData("A02", "packageProductRef", "WEIGHT_PACK_10")]
    [InlineData("A24", "planCode", "WIFI_FULL_FLIGHT")]
    public async Task S2_W03_W04_the_package_or_plan_option_is_the_authored_sku_only_and_a_foreign_sku_is_rejected(string code, string fieldName, string sku)
    {
        var lab = new ShoppingLab();
        var definition = lab.Definition(code);
        var provision = lab.Provision(definition);

        lab.Price(definition, provision, Rate(100m));

        var context = code == "A02" ? Trip.Context() with { TravellerBaggageFacts = [Trip.WeightAllowance()] } : Trip.Context();
        var candidate = (await lab.Engine.ShopAsync(context, new ShoppingFilter { IncludeNonSellable = true })).Candidates.First(row => row.VariantCode == code);
        var groups = OfferOptionCatalog.Build(definition, provision, candidate, context);
        var group = Assert.Single(groups);
        var foreign = code == "A02"
            ? new AncillarySelection { PackageProductRef = "WEIGHT_PACK_20", Quantity = 1 }
            : new AncillarySelection { PlanCode = "FullFlight", DeviceCount = 1 };
        var refused = await lab.Engine.EvaluateSelectionAsync(context, candidate.CandidateIdentity, foreign);

        Assert.Equal((fieldName, OptionSourceState.Known), (group.FieldName, group.SourceState));
        Assert.Equal(sku, Assert.Single(group.Choices).Value);
        Assert.Equal(SelectionEvaluationStatus.Rejected, refused.Status);
        Assert.Contains(refused.FieldIssues, issue => issue.ReasonCode == ShoppingReasonCodes.SelectedOptionNotOffered);
    }

    [Fact]
    public async Task S2_W05_an_authored_menu_item_is_the_single_known_choice_any_other_dish_is_rejected_and_a_paid_meal_without_a_menu_cannot_be_authored()
    {
        var lab = new ShoppingLab();
        var authored = lab.Definition("A12");
        var authoredProvision = lab.Provision(authored);

        lab.Price(authored, authoredProvision, Rate(20m));

        var context = Trip.Context();
        var known = (await lab.Engine.ShopAsync(context, new ShoppingFilter { IncludeNonSellable = true })).Candidates.First(candidate => candidate.VariantCode == "A12");
        var knownGroup = Assert.Single(OfferOptionCatalog.Build(authored, authoredProvision, known, context));
        var arbitrary = await lab.Engine.EvaluateSelectionAsync(context, known.CandidateIdentity, new AncillarySelection { MenuItemRef = "ANY_DISH", Quantity = 1 });
        var unauthorable = Assert.Throws<BusinessException>(
            () => lab.Definition("A12", reference: "MEAL_NO_MENU", specification: input => input with { Meal = input.Meal! with { MenuItemRef = null } }));

        Assert.Equal(("menuItemRef", OptionSourceState.Known, "MENU_PASTA_01"), (knownGroup.FieldName, knownGroup.SourceState, Assert.Single(knownGroup.Choices).Value));
        Assert.Equal(SelectionEvaluationStatus.Rejected, arbitrary.Status);
        Assert.Contains(arbitrary.FieldIssues, issue => issue.ReasonCode == ShoppingReasonCodes.MenuItemMismatch);
        Assert.False(OfferPurchase.CanPurchase(arbitrary));
        Assert.Contains("MenuItemRef", unauthorable.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("channel")]
    [InlineData("customer")]
    [InlineData("country")]
    public async Task S2_W11b_a_priced_offer_whose_commercial_section_does_not_match_the_caller_is_not_sellable(string tampered)
    {
        var offer = (await OfferWire.ReadAsync(OfferWire.Captured())) with { SalesChannel = Channel.PartnerAPI, CustomerId = 501, SalesCountryId = 7 };
        var scope = new AncillarySalesScope(Airline, Channel.PartnerAPI, [Pos], TravelAgencyId: 900, CustomerId: 501, CountryId: 7);
        var sold = tampered switch
        {
            "channel" => offer with { SalesChannel = Channel.AgencyPanel },
            "customer" => offer with { CustomerId = 502 },
            _ => offer with { SalesCountryId = 8 }
        };
        var service = Service(new ShoppingLab(), sold);
        var accepted = await Service(new ShoppingLab(), offer).ExecuteAsync(new OfferQuery(OfferWire.OfferId), scope);
        var refused = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteAsync(new OfferQuery(OfferWire.OfferId), scope));

        Assert.Equal(OfferWire.OfferId, accepted.Offer.OfferId);
        Assert.Equal((16809, 403), (refused.Code, refused.HttpStatus));
    }

    [Fact]
    public async Task S2_W11_an_offer_item_ref_of_one_agency_is_not_found_by_another_agency_with_the_same_point_of_sale()
    {
        var lab = new ShoppingLab();

        lab.Catalog.CurrencyScales[Irr] = 0;

        var bag = lab.Definition("A01");

        lab.Price(bag, lab.Provision(bag), Rate(5_000_000m, Irr));

        var offer = (await OfferWire.ReadAsync(OfferWire.Captured())) with { SalesChannel = Channel.PartnerAPI, CustomerId = null };
        var service = Service(lab, offer);
        var agencyA = new AncillarySalesScope(Airline, Channel.PartnerAPI, [Pos], TravelAgencyId: 900);
        var agencyB = agencyA with { TravelAgencyId = 901 };
        var shopped = await service.ExecuteAsync(new OfferQuery(OfferWire.OfferId), agencyA);
        var item = shopped.Items.First();
        var foreign = await Assert.ThrowsAsync<BusinessException>(() => service.FindItemAsync(new OfferQuery(OfferWire.OfferId, item.OfferItemRef), agencyB));
        var (_, own) = await service.FindItemAsync(new OfferQuery(OfferWire.OfferId, item.OfferItemRef), agencyA);

        Assert.Equal((16811, 404), (foreign.Code, foreign.HttpStatus));
        Assert.Equal(item.Candidate.CandidateIdentity, own.Candidate.CandidateIdentity);
    }

    [Fact]
    public async Task S2_W21_the_source_fingerprint_changes_with_allowance_coupon_flags_prices_order_charges_and_rates_of_exchange()
    {
        var offer = await OfferWire.ReadAsync(OfferWire.Captured());
        var baseline = FlightOfferFingerprint.Of(offer);
        var coupon = offer.Tickets[0].Coupons[0];

        FlightOffer WithCoupon(FlightOfferCoupon changed)
            => offer with { Tickets = [offer.Tickets[0] with { Coupons = [changed, .. offer.Tickets[0].Coupons.Skip(1)] }, .. offer.Tickets.Skip(1)] };

        var changes = new[]
        {
            WithCoupon(coupon with { CheckedBaggage = coupon.CheckedBaggage! with { Weight = coupon.CheckedBaggage.Weight + 5 } }),
            WithCoupon(coupon with { IsUpgradable = !coupon.IsUpgradable }),
            WithCoupon(coupon with { PriceLines = [coupon.PriceLines[0] with { Amount = coupon.PriceLines[0].Amount + 1 }, .. coupon.PriceLines.Skip(1)] }),
            offer with { OrderCharges = [] },
            offer with { RatesOfExchange = [offer.RatesOfExchange[0] with { Rate = offer.RatesOfExchange[0].Rate + 1 }, .. offer.RatesOfExchange.Skip(1)] }
        };

        Assert.Equal(baseline, FlightOfferFingerprint.Of(offer with { }));
        Assert.All(changes, changed => Assert.NotEqual(baseline, FlightOfferFingerprint.Of(changed)));
    }

    [Fact]
    public async Task S2_W12_an_order_scoped_service_is_listed_once_at_the_top_of_the_ota_panel_answer_and_never_under_a_traveller()
    {
        var lab = new ShoppingLab();

        lab.Catalog.CurrencyScales[Irr] = 0;

        var wifi = lab.Definition("A24");
        var provision = lab.Provision(wifi, scope: ServiceCoverageScope.Order);

        lab.Price(wifi, provision, Rate(100m, Irr));
        lab.Policy(wifi, InventoryAuthority.Supplier);

        var snapshot = await SnapshotAsync(lab, scope: new DeskScope(Airline, Pos, null, null, Now).On(AgencySalesPolicy.SalesChannel, Pos));
        var panel = OtaPanelAncillaryOffersMapper.Map(snapshot);
        var order = Assert.Single(panel.OrderScopedServices);

        Assert.Equal(ServiceCoverageScope.Order.ToString(), order.CoverageScope);
        Assert.DoesNotContain(panel.TravellerGroups.SelectMany(group => group.FlightGroups).SelectMany(flight => flight.Services), service => service.OfferItemRef == order.OfferItemRef);
        Assert.True(panel.TravellerGroups.Count >= 2);
    }

    private const string CoveragePlannerOrderRef = "ORDER";
    private const int Irr = 70;

    private static GetAncillaryOffersService Service(ShoppingLab lab, FlightOffer offer)
        => new(
            Sources(new FlightOfferShoppingSource(
                new FixedOffers(offer),
                new TestOnlyAirportReference(),
                new TestOnlyAirFareReference(),
                new FixedClock { Now = Now },
                new FlightOfferShoppingOptions(SourceContextTtl))),
            lab.Engine);
}
