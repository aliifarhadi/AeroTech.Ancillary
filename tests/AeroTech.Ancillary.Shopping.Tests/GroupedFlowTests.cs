using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Providers.Orders;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.SelectionIds;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections.Service;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.OrderDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class GroupedFlowTests
{
    private const int Irr = 70;
    private const string Adult = "ADT-1";

    private static readonly DeskScope Desk = new(Airline, Pos, null, null, Now);
    private static readonly OfferQuery ByOffer = new(OfferWire.OfferId);
    private static readonly OfferQuery ByOrder = new(null, OrderId: OrderDesk.OrderId);

    private static ShoppingLab Shop(FlightOffer? local = null)
    {
        var lab = new ShoppingLab();

        lab.Catalog.CurrencyScales[Irr] = 0;

        var bag = lab.Definition("A01");
        var wifi = lab.Definition("A24");
        var partnerWifi = lab.Definition("A24", "WIFI_PARTNER");
        var extraSeat = lab.Definition("A09");
        var localBag = lab.Definition("A01", "BAG_LOCAL", activate: local is not null);
        var priority = lab.Definition("A23", specification: input => input with { Priority = input.Priority! with { FareBenefitRef = null, AirportIds = null } });

        lab.Price(bag, lab.Provision(bag, stage: PurchaseStage.Both), Rate(5_000_000m, Irr));
        lab.Price(wifi, lab.Provision(wifi, stage: PurchaseStage.Both), Rate(1_500_000m, Irr));
        lab.Price(partnerWifi, lab.Provision(partnerWifi, stage: PurchaseStage.Both), Rate(900_000m, Irr));
        lab.Price(extraSeat, lab.Provision(extraSeat, stage: PurchaseStage.Both), Rate(9_000_000m, Irr));
        lab.Price(localBag, lab.Provision(localBag, stage: PurchaseStage.Both), Rate(4_000_000m, Irr));
        lab.Price(priority, lab.Provision(priority, stage: PurchaseStage.Both), Rate(700_000m, Irr));
        lab.Policy(priority, InventoryAuthority.Unlimited);
        lab.Policy(bag, InventoryAuthority.Unlimited);
        lab.Policy(wifi, InventoryAuthority.Unlimited);
        lab.Policy(partnerWifi, InventoryAuthority.Supplier);
        lab.Policy(
            localBag,
            InventoryAuthority.Local,
            LocalInventoryPattern.FlightCount,
            FlightCountConsumption.Create(990_001, 1, InventoryCountUnit.Item),
            flightSources: (local?.Bounds.SelectMany(bound => bound.Flights) ?? []).Select(flight => new FlightInventorySource(InventoryResourceKind.FlightCount, flight.FlightId, 990_001, InventoryRecordStatus.Active, false)).ToList());
        lab.Catalog.ResourceAnswer = InventoryReferenceCheck.Verified;

        return lab;
    }

    private static async Task<FlightOffer> AirAsync() => Desk.Sell(await OfferWire.ReadAsync(OfferWire.Captured()));

    private static BackofficeGroupedOfferDto Item(BackofficeGroupedAncillaryOffersDto grouped, string variantCode, string? reference = null, string traveller = Adult)
    {
        var all = grouped.Baggages.Cast<BackofficeGroupedOfferDto>().Concat(grouped.Meals).Concat(grouped.PriorityServices).Concat(grouped.Connectivity).Concat(grouped.ExtraSeats).ToList();

        return all.FirstOrDefault(item => item.VariantCode == variantCode && (reference is null || item.ServiceDefinitionRef == reference) && item.TravellerRefs.SequenceEqual([traveller]))
               ?? throw new InvalidOperationException($"{variantCode}/{traveller} not in [{string.Join(", ", all.Select(item => $"{item.VariantCode}:{string.Join('+', item.TravellerRefs)}:{item.Readiness.Name}"))}]");
    }

    private static async Task<BackofficeGroupedAncillaryOffersDto> ListAsync(GroupedDesk desk, OfferQuery? query = null)
        => BackofficeGroupedAncillaryOffersMapper.Map(await desk.Grouped.ExecuteAsync(query ?? ByOffer, Desk));

    private static async Task<BackofficeGroupedOfferDetailDto> DetailAsync(GroupedDesk desk, string offerItemRef, AncillaryProfile family, AncillarySelection? selection, OfferQuery? query = null)
        => BackofficeGroupedOfferDetailMapper.Map(await desk.Details.ExecuteAsync(new OfferQuery((query ?? ByOffer).OfferId, offerItemRef, (query ?? ByOffer).OrderId), family, selection, Desk));

    private static GroupedSelectionRequest Resolve(string? offerId, long? orderId, int? version, params (AncillaryProfile Profile, string Id)[] ids)
        => new(offerId, orderId, version, [], ids.Select(id => new GroupedSelectionEntry(id.Profile, id.Id)).ToList());

    private static string Tampered(string payload, string from, string to) => Base64Url.EncodeToString(Encoding.UTF8.GetBytes(payload.Replace(from, to)));

    [Fact]
    public void GRP_B01_the_selection_id_round_trips_canonically_and_rejects_tampering_and_internal_ids()
    {
        var id = new NonSeatSelectionId(
            ShoppingSourceKind.Offer,
            OfferWire.OfferId,
            null,
            new NonSeatSelectionScope(Channel.BackOffice, Pos, BackofficeOfficeId, null, null, null),
            "hash",
            AncillaryProfile.Baggage,
            "A01",
            new AncillarySelection { Quantity = 2, Dimensions = new SelectedDimensions(10.5m, 20m, 30m), DocumentAcknowledgements = ["b", "a", "a"] });
        var encoded = NonSeatSelectionIdCodec.Encode(id);
        var decoded = NonSeatSelectionIdCodec.Decode(encoded)!;
        var payload = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(encoded));

        Assert.Equal(id with { Selection = decoded.Selection }, decoded);
        Assert.Equal(2, decoded.Selection.Quantity);
        Assert.Equal(["a", "b"], decoded.Selection.DocumentAcknowledgements);
        Assert.Equal(new SelectedDimensions(10.5m, 20m, 30m), decoded.Selection.Dimensions);
        Assert.Equal(encoded, NonSeatSelectionIdCodec.Encode(decoded));
        Assert.DoesNotContain("provisionId", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("pricing", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("definition", payload, StringComparison.OrdinalIgnoreCase);
        Assert.Null(NonSeatSelectionIdCodec.Decode(encoded[..^4] + "AAAA"));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Tampered(payload, "\"v\":1", "\"v\":2")));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Tampered(payload, "\"selection\":{", "\"selection\":{\"provisionId\":5,")));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Tampered(payload, "\"profile\":\"Baggage\"", "\"profile\":\"Meal\"")));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Base64Url.EncodeToString(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(payload), new JsonSerializerOptions { WriteIndented = true })))));
    }

    [Fact]
    public async Task GRP_B02_the_list_issues_ids_only_for_deterministic_choices_and_always_points_to_a_real_details_route()
    {
        var desk = await GroupedDesk.CreateAsync(Shop(), await AirAsync());
        var grouped = await ListAsync(desk);
        var bag = Item(grouped, "A01", "EXTRA_BAG");
        var wifi = Item(grouped, "A24", "WIFI_FULL_FLIGHT");
        var partnerWifi = Item(grouped, "A24", "WIFI_PARTNER");
        var extraSeat = Item(grouped, "A09");
        var decoded = NonSeatSelectionIdCodec.Decode(wifi.SelectionId)!;

        Assert.Null(bag.SelectionId);
        Assert.Equal((1, 6, "Piece", false, "/Backoffice/v1/AncillaryOffers/Baggages/Details"), (bag.Quantity!.Min, bag.Quantity.Max, bag.Quantity.Unit.Name, bag.RequiresDetails, bag.DetailsEndpoint));
        Assert.NotNull(wifi.SelectionId);
        Assert.NotNull(partnerWifi.SelectionId);
        Assert.All(new[] { wifi, partnerWifi }, item => Assert.Null(item.DetailsEndpoint));
        Assert.Equal((true, 1_500_000m, (BackofficeGroupedQuantityDto?)null), (wifi.Price.IsTotalComplete, wifi.Price.UnitTotal, wifi.Quantity));
        Assert.Equal(("A09", (string?)null, "/Backoffice/v1/AncillaryOffers/ExtraSeats/Details"), (extraSeat.VariantCode, extraSeat.SelectionId, extraSeat.DetailsEndpoint));
        Assert.Equal(new AncillarySelection { PlanCode = "WIFI_FULL_FLIGHT" }, decoded.Selection);
        Assert.Equal((ShoppingSourceKind.Offer, OfferWire.OfferId, wifi.OfferItemRef, AncillaryProfile.Connectivity, "A24"), (decoded.SourceKind, decoded.SourceRef, decoded.OfferItemRef, decoded.Profile, decoded.VariantCode));
    }

    [Fact]
    public async Task GRP_B03_details_validate_a_typed_choice_through_the_engine_and_give_the_same_id_as_the_list()
    {
        var desk = await GroupedDesk.CreateAsync(Shop(), await AirAsync());
        var grouped = await ListAsync(desk);
        var bag = Item(grouped, "A01", "EXTRA_BAG");
        var wifi = Item(grouped, "A24", "WIFI_FULL_FLIGHT");

        var form = await DetailAsync(desk, bag.OfferItemRef, AncillaryProfile.Baggage, null);
        var chosen = await DetailAsync(desk, bag.OfferItemRef, AncillaryProfile.Baggage, new AncillarySelection { Quantity = 2 });
        var tooMany = await DetailAsync(desk, bag.OfferItemRef, AncillaryProfile.Baggage, new AncillarySelection { Quantity = 99 });
        var foreign = await DetailAsync(desk, bag.OfferItemRef, AncillaryProfile.Baggage, new AncillarySelection { Quantity = 1, MealCode = "VGML" });
        var direct = await DetailAsync(desk, wifi.OfferItemRef, AncillaryProfile.Connectivity, null);

        Assert.Equal((null, "quantity", true, "Integer"), (form.SelectionId, form.RequiredFields.Single().Name, form.RequiredFields.Single().Required, form.RequiredFields.Single().Type.Name));
        Assert.NotNull(chosen.SelectionId);
        Assert.Equal((2, "Selectable", true, 5_000_000m), (((BackofficeGroupedBaggageSelectedDto)chosen.Selected!).Quantity, chosen.Readiness.Name, chosen.Price.IsTotalComplete, chosen.Price.UnitTotal));
        Assert.Equal(new AncillarySelection { Quantity = 2 }, NonSeatSelectionIdCodec.Decode(chosen.SelectionId)!.Selection);
        Assert.Equal((null, true), (tooMany.SelectionId, tooMany.ReasonCodes.Contains(ShoppingReasonCodes.QuantityAboveMaximum)));
        Assert.Equal((null, true), (foreign.SelectionId, foreign.ReasonCodes.Contains(ShoppingReasonCodes.SelectionFieldNotAllowed)));
        Assert.Equal(wifi.SelectionId, direct.SelectionId);
        await Assert.ThrowsAsync<BusinessException>(() => DetailAsync(desk, bag.OfferItemRef, AncillaryProfile.Meal, null));
        await Assert.ThrowsAsync<BusinessException>(() => DetailAsync(desk, bag.OfferItemRef, AncillaryProfile.Baggage, new AncillarySelection { Quantity = 1 }, new OfferQuery(OfferWire.OfferId, null, OrderDesk.OrderId)));
    }

    [Fact]
    public async Task GRP_B04_resolve_grouped_revalidates_offer_ids_and_rejects_forged_foreign_or_misfiled_ones_without_side_effects()
    {
        var desk = await GroupedDesk.CreateAsync(Shop(), await AirAsync());
        var grouped = await ListAsync(desk);
        var bag = await DetailAsync(desk, Item(grouped, "A01", "EXTRA_BAG").OfferItemRef, AncillaryProfile.Baggage, new AncillarySelection { Quantity = 2 });
        var wifi = Item(grouped, "A24", "WIFI_FULL_FLIGHT");

        var valid = await desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Baggage, bag.SelectionId!), (AncillaryProfile.Connectivity, wifi.SelectionId!)));
        var misfiled = await desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Meal, bag.SelectionId!)));
        var forged = await desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Baggage, bag.SelectionId![..^3] + "xyz")));
        var otherOffer = await desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Baggage, NonSeatSelectionIdCodec.Encode(NonSeatSelectionIdCodec.Decode(bag.SelectionId)! with { SourceRef = "other-offer" }))));

        Assert.True(valid.AllValid);
        Assert.Equal(new[] { SelectionResolutionStatus.Valid, SelectionResolutionStatus.Valid }, valid.Selections.Select(selection => selection.Status));
        Assert.Equal((10_000_000m, 2, Irr), (valid.Selections[0].Line!.Total, valid.Selections[0].Line!.Candidate!.Price.Units, valid.Selections[0].Line!.CurrencyId));
        Assert.Equal((false, SelectionResolutionStatus.Rejected, ShoppingReasonCodes.OfferItemNotOffered), (misfiled.AllValid, misfiled.Selections[0].Status, misfiled.Selections[0].ReasonCodes.Single()));
        Assert.Equal((SelectionResolutionStatus.Rejected, ShoppingReasonCodes.PricedSelectionNotAuthentic), (forged.Selections[0].Status, forged.Selections[0].ReasonCodes.Single()));
        Assert.Equal((SelectionResolutionStatus.Rejected, ShoppingReasonCodes.PricedSelectionForAnotherOrder), (otherOffer.Selections[0].Status, otherOffer.Selections[0].ReasonCodes.Single()));
        await Assert.ThrowsAsync<BusinessException>(() => desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, OrderDesk.OrderId, 1, (AncillaryProfile.Baggage, bag.SelectionId!))));
        await Assert.ThrowsAsync<BusinessException>(() => desk.Resolve.ExecuteAsync(Resolve(null, OrderDesk.OrderId, null, (AncillaryProfile.Baggage, bag.SelectionId!))));
        await Assert.ThrowsAsync<BusinessException>(() => desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Baggage, bag.SelectionId!), (AncillaryProfile.Baggage, bag.SelectionId!))));
    }

    [Fact]
    public async Task GRP_B05_resolve_grouped_on_an_order_accepts_order_issued_ids_and_marks_offer_issued_or_stale_ones()
    {
        var lab = Shop();
        var air = await AirAsync();
        var orders = new FixedOrders(OrderOf(air));
        var desk = await GroupedDesk.CreateAsync(lab, air, orders);
        var offerIssued = Item(await ListAsync(desk), "A24", "WIFI_FULL_FLIGHT").SelectionId!;
        var orderIssued = Item(await ListAsync(desk, ByOrder), "A24", "WIFI_FULL_FLIGHT").SelectionId!;
        var decoded = NonSeatSelectionIdCodec.Decode(orderIssued)!;

        var valid = await desk.Resolve.ExecuteAsync(Resolve(null, OrderDesk.OrderId, 1, (AncillaryProfile.Connectivity, orderIssued)));
        var foreign = await desk.Resolve.ExecuteAsync(Resolve(null, OrderDesk.OrderId, 1, (AncillaryProfile.Connectivity, offerIssued)));
        var stale = await desk.Resolve.ExecuteAsync(Resolve(null, OrderDesk.OrderId, 2, (AncillaryProfile.Connectivity, orderIssued)));

        Assert.Equal((ShoppingSourceKind.Order, "7001", 1), (decoded.SourceKind, decoded.SourceRef, decoded.OrderCommercialVersion));
        Assert.Equal((true, (long?)OrderDesk.OrderId, (int?)1), (valid.AllValid, valid.OrderId, valid.OrderCommercialVersion));
        Assert.Equal([101L], valid.Selections[0].Context!.Travellers.Where(traveller => traveller.TravellerRef == Adult).Select(traveller => traveller.OrderTravellerId!.Value));
        Assert.Equal((SelectionResolutionStatus.Rejected, ShoppingReasonCodes.PricedSelectionForAnotherOrder), (foreign.Selections[0].Status, foreign.Selections[0].ReasonCodes.Single()));
        Assert.Equal((SelectionResolutionStatus.Stale, ShoppingReasonCodes.OrderVersionChanged), (stale.Selections[0].Status, stale.Selections[0].ReasonCodes.Single()));
    }

    private static OrderServiceFacts Accepted(long orderServiceId, string id, ServiceResolvedServiceDto service, string traveller = Adult, string? portion = "B1", string[]? flights = null, int? quantity = null, ExistingServiceCommercialState state = ExistingServiceCommercialState.Active)
        => new(orderServiceId, traveller, service.ServiceDefinitionRef, portion, flights ?? ["B1:F1"], quantity ?? service.Price.Quantity!.Value, state, id, service.Price.CurrencyId, service.Price.LineTotal, service.DefinitionVersionId, service.ProvisionId, service.PricingRevisionId);

    private static GroupedSelectionVerification Verification(int version, params (AncillaryProfile Profile, long OrderServiceId, string Id)[] entries)
        => new(OrderDesk.OrderId, version, Pos, Irr, null, entries.Select(entry => new GroupedHoldSelectionEntry(entry.Profile, entry.OrderServiceId, entry.Id)).ToList());

    private static ServiceResolvedServiceDto ServiceOf(GroupedSelectionResolutionSnapshot resolution, int index)
    {
        var mapped = ServiceGroupedSelectionResolutionMapper.Map(resolution);

        return mapped.Baggages.Concat(mapped.Connectivity).ElementAt(index).Service!;
    }

    private static async Task RefusedAsync(GroupedDesk desk, GroupedSelectionVerification verification, string reason)
    {
        var exception = await Assert.ThrowsAsync<BusinessException>(() => desk.Verifier.VerifyAsync(verification));

        Assert.Equal(16402, exception.Code);
        Assert.True(exception.Message.Contains(reason, StringComparison.Ordinal), exception.Message);
    }

    [Fact]
    public async Task GRP_B06_the_hold_verifier_binds_each_order_service_to_its_accepted_snapshot_and_fails_closed_otherwise()
    {
        var lab = Shop();
        var air = await AirAsync();
        var probe = await GroupedDesk.CreateAsync(lab, air);
        var grouped = await ListAsync(probe);
        var bag = (await DetailAsync(probe, Item(grouped, "A01", "EXTRA_BAG").OfferItemRef, AncillaryProfile.Baggage, new AncillarySelection { Quantity = 2 })).SelectionId!;
        var partnerWifi = Item(grouped, "A24", "WIFI_PARTNER").SelectionId!;
        var wifi = Item(grouped, "A24", "WIFI_FULL_FLIGHT").SelectionId!;
        var partnerDefinition = lab.Catalog.Definitions.Single(definition => definition.ServiceDefinitionRef == "WIFI_PARTNER");
        var partnerProvision = lab.Catalog.Provisions.Single(provision => provision.ServiceDefinitionId == partnerDefinition.Id);
        var partnerPricing = lab.Catalog.Pricings.Single(pricing => pricing.AncillaryProvisionId == partnerProvision.Id);
        var resolved = await probe.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Baggage, bag), (AncillaryProfile.Connectivity, wifi)));
        var bagService = ServiceOf(resolved, 0);
        var wifiService = ServiceOf(resolved, 1);
        var desk = await GroupedDesk.CreateAsync(lab, air, new FixedOrders(OrderOf(
            air,
            services:
            [
                Accepted(2001, bag, bagService),
                Accepted(2002, partnerWifi, wifiService with { DefinitionVersionId = partnerDefinition.Id, ProvisionId = partnerProvision.Id, PricingRevisionId = partnerPricing.Id, Price = wifiService.Price with { LineTotal = 900_000m } }),
                Accepted(2003, wifi, wifiService),
                Accepted(2004, bag, bagService with { Price = bagService.Price with { LineTotal = 9_999_999m } }),
                Accepted(2005, bag, bagService, traveller: "CHD-1"),
                Accepted(2006, bag, bagService, flights: ["B2:F1"], portion: "B2"),
                Accepted(2007, bag, bagService, state: ExistingServiceCommercialState.Cancelled),
                Accepted(2008, bag, bagService, quantity: 1),
                Accepted(2010, bag, bagService with { PricingRevisionId = 42 }),
                Accepted(2011, bag, bagService with { DefinitionVersionId = wifiService.DefinitionVersionId, ProvisionId = wifiService.ProvisionId })
            ])));

        var verified = await desk.Verifier.VerifyAsync(Verification(1, (AncillaryProfile.Baggage, 2001, bag), (AncillaryProfile.Connectivity, 2003, wifi)));

        Assert.True(resolved.AllValid);
        Assert.Equal(
            new[] { (2001L, (long?)101L, ServiceCoverageScope.Portion, 2), (2003L, 101L, ServiceCoverageScope.Sector, 1) },
            verified.Select(selection => (selection.OrderServiceId, selection.TravellerId, selection.CoverageScope, selection.Quantity)));
        Assert.Equal([1558420526869774453L], verified[0].CoveredFlightIds);
        Assert.Equal(bag, verified[0].SelectionId);
        await RefusedAsync(desk, Verification(1, (AncillaryProfile.Connectivity, 2002, partnerWifi)), ShoppingReasonCodes.InventoryDelegatedCheckRequired);
        await RefusedAsync(desk, Verification(1, (AncillaryProfile.Baggage, 2002, bag)), "AcceptedSelectionId");
        await RefusedAsync(desk, Verification(1, (AncillaryProfile.Baggage, 2004, bag)), ShoppingReasonCodes.PriceChanged);
        Assert.Equal(102L, (await desk.Verifier.VerifyAsync(Verification(1, (AncillaryProfile.Baggage, 2005, bag))))[0].TravellerId);
        Assert.Equal(
            [air.Bounds[1].Flights[0].FlightId],
            (await desk.Verifier.VerifyAsync(Verification(1, (AncillaryProfile.Baggage, 2006, bag))))[0].CoveredFlightIds);
        await RefusedAsync(desk, Verification(1, (AncillaryProfile.Baggage, 2007, bag)), "CommercialState");
        await RefusedAsync(desk, Verification(1, (AncillaryProfile.Baggage, 2008, bag)), "Quantity");
        await RefusedAsync(desk, Verification(1, (AncillaryProfile.Baggage, 2009, bag)), "OrderServiceId");
        await RefusedAsync(desk, Verification(1, (AncillaryProfile.Baggage, 2010, bag)), ShoppingReasonCodes.PriceChanged);
        await RefusedAsync(desk, Verification(1, (AncillaryProfile.Baggage, 2011, bag)), ShoppingReasonCodes.OfferItemNotOffered);
        await RefusedAsync(desk, Verification(2, (AncillaryProfile.Baggage, 2001, bag)), "OrderCommercialVersion");
        await RefusedAsync(desk, Verification(1, (AncillaryProfile.Baggage, 2001, bag)) with { PointOfSaleId = OtherPos }, ShoppingReasonCodes.PointOfSaleNotAllowed);

        var disconnected = await GroupedDesk.CreateAsync(lab, air, new NotConnectedOrderCommercialContextReader());

        Assert.Equal(16815, (await Assert.ThrowsAsync<BusinessException>(() => disconnected.Verifier.VerifyAsync(Verification(1, (AncillaryProfile.Baggage, 2001, bag))))).Code);
    }

    [Fact]
    public async Task GRP_B07_an_id_issued_on_order_v1_still_holds_after_the_accepted_service_moved_the_order_to_v2()
    {
        var lab = Shop();
        var air = await AirAsync();
        var before = await GroupedDesk.CreateAsync(lab, air, new FixedOrders(OrderOf(air)));
        var bag = (await DetailAsync(before, Item(await ListAsync(before, ByOrder), "A01", "EXTRA_BAG").OfferItemRef, AncillaryProfile.Baggage, new AncillarySelection { Quantity = 2 }, ByOrder)).SelectionId!;
        var resolved = await before.Resolve.ExecuteAsync(Resolve(null, OrderDesk.OrderId, 1, (AncillaryProfile.Baggage, bag)));
        var service = ServiceOf(resolved, 0);
        var after = await GroupedDesk.CreateAsync(lab, air, new FixedOrders(OrderOf(air, commercialVersion: 2, services: [Accepted(3001, bag, service)])));

        var verified = await after.Verifier.VerifyAsync(Verification(2, (AncillaryProfile.Baggage, 3001, bag)));
        var resolvedAgain = await after.Resolve.ExecuteAsync(Resolve(null, OrderDesk.OrderId, 2, (AncillaryProfile.Baggage, bag)));

        Assert.True(resolved.AllValid);
        Assert.Equal((3001L, (long?)101L, 2, service.DefinitionVersionId, service.ProvisionId), (verified[0].OrderServiceId, verified[0].TravellerId, verified[0].Quantity, verified[0].ServiceDefinitionId, verified[0].ProvisionId));
        Assert.Equal((false, SelectionResolutionStatus.Stale), (resolvedAgain.AllValid, resolvedAgain.Selections[0].Status));
        await Assert.ThrowsAsync<BusinessException>(() => after.Verifier.VerifyAsync(Verification(1, (AncillaryProfile.Baggage, 3001, bag))));
    }

    [Fact]
    public async Task GRP_B08_resolve_grouped_never_validates_what_the_grouped_hold_cannot_take()
    {
        var air = await AirAsync();
        var desk = await GroupedDesk.CreateAsync(Shop(air), air);
        var grouped = await ListAsync(desk);
        var partnerWifi = Item(grouped, "A24", "WIFI_PARTNER");
        var wifi = Item(grouped, "A24", "WIFI_FULL_FLIGHT");
        var localBag = Item(grouped, "A01", "BAG_LOCAL");
        var extraSeat = Item(grouped, "A09");
        var template = NonSeatSelectionIdCodec.Decode(wifi.SelectionId)!;
        var localId = NonSeatSelectionIdCodec.Encode(template with { OfferItemRef = localBag.OfferItemRef, Profile = AncillaryProfile.Baggage, VariantCode = "A01", Selection = new AncillarySelection { Quantity = 1 } });
        var extraSeatId = NonSeatSelectionIdCodec.Encode(template with { OfferItemRef = extraSeat.OfferItemRef, Profile = AncillaryProfile.Seat, VariantCode = "A09", Selection = new AncillarySelection { SeatNumber = "3A" } });

        var supplier = await desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Connectivity, partnerWifi.SelectionId!)));
        var local = await desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Baggage, localId)));
        var seat = await desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Seat, extraSeatId)));
        var unlimited = await desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Connectivity, wifi.SelectionId!)));

        Assert.NotNull(partnerWifi.SelectionId);
        Assert.Equal((false, SelectionResolutionStatus.Unavailable, ShoppingReasonCodes.InventoryDelegatedCheckRequired), (supplier.AllValid, supplier.Selections[0].Status, supplier.Selections[0].ReasonCodes.Single()));
        Assert.False(local.AllValid);
        Assert.NotEqual(SelectionResolutionStatus.Valid, local.Selections[0].Status);
        Assert.Equal((false, SelectionResolutionStatus.Unavailable, ShoppingReasonCodes.SeatSelectionNotOffered), (seat.AllValid, seat.Selections[0].Status, seat.Selections[0].ReasonCodes.Single()));
        Assert.True(unlimited.AllValid);
    }

    [Fact]
    public void GRP_B09_a_malformed_or_oversized_payload_is_rejected_without_an_exception()
    {
        var payload = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(NonSeatSelectionIdCodec.Encode(new NonSeatSelectionId(
            ShoppingSourceKind.Offer, OfferWire.OfferId, null, new NonSeatSelectionScope(Channel.BackOffice, Pos, BackofficeOfficeId, null, null, null), "hash", AncillaryProfile.Baggage, "A01", new AncillarySelection { Quantity = 2 }))));

        Assert.Contains("\"pointOfSaleId\":9001", payload);
        Assert.Null(NonSeatSelectionIdCodec.Decode(Tampered(payload, "\"v\":1", "\"v\":99999999999")));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Tampered(payload, "\"v\":1", "\"v\":\"1\"")));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Tampered(payload, "\"pointOfSaleId\":9001", "\"pointOfSaleId\":\"9001\"")));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Tampered(payload, "\"pointOfSaleId\":9001", "\"pointOfSaleId\":99999999999999999999")));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Tampered(payload, "\"selection\":{\"quantity\":2}", "\"selection\":[2]")));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Tampered(payload, "\"quantity\":2", "\"quantity\":99999999999")));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Tampered(payload, "\"quantity\":2", "\"quantity\":2.5")));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Base64Url.EncodeToString(Encoding.UTF8.GetBytes("[1,2]"))));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Base64Url.EncodeToString(Encoding.UTF8.GetBytes("{\"v\":1"))));
        Assert.Null(NonSeatSelectionIdCodec.Decode(Base64Url.EncodeToString(new byte[] { 0xFF, 0xFE, 0x00 })));
        Assert.Null(NonSeatSelectionIdCodec.Decode(new string('A', 5000)));
    }

    [Fact]
    public async Task GRP_B11_a_consent_field_is_never_filled_for_the_customer_and_only_an_explicit_opt_in_earns_an_id()
    {
        var desk = await GroupedDesk.CreateAsync(Shop(), await AirAsync());
        var grouped = await ListAsync(desk);
        var priority = Item(grouped, "A23");
        var wifi = Item(grouped, "A24", "WIFI_FULL_FLIGHT");
        var optedIn = await DetailAsync(desk, priority.OfferItemRef, AncillaryProfile.Priority, new AncillarySelection { OptIn = true });
        var declined = await DetailAsync(desk, priority.OfferItemRef, AncillaryProfile.Priority, new AncillarySelection { OptIn = false });
        var silent = await DetailAsync(desk, priority.OfferItemRef, AncillaryProfile.Priority, new AncillarySelection());
        var untouched = await DetailAsync(desk, priority.OfferItemRef, AncillaryProfile.Priority, null);
        var forged = NonSeatSelectionIdCodec.Encode(NonSeatSelectionIdCodec.Decode(optedIn.SelectionId)! with { Selection = new AncillarySelection { OptIn = false } });
        var resolved = await desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Priority, optedIn.SelectionId!)));
        var rejected = await desk.Resolve.ExecuteAsync(Resolve(OfferWire.OfferId, null, null, (AncillaryProfile.Priority, forged)));

        Assert.Equal(((string?)null, true, "/Backoffice/v1/AncillaryOffers/PriorityServices/Details"), (priority.SelectionId, priority.RequiresDetails, priority.DetailsEndpoint));
        Assert.NotNull(optedIn.SelectionId);
        Assert.Equal(new AncillarySelection { OptIn = true }, NonSeatSelectionIdCodec.Decode(optedIn.SelectionId)!.Selection);
        Assert.All(new[] { declined, silent, untouched }, detail => Assert.Null(detail.SelectionId));
        Assert.Contains(ShoppingReasonCodes.SelectionFieldRequired, declined.ReasonCodes);
        Assert.Contains(ShoppingReasonCodes.SelectionFieldRequired, silent.ReasonCodes);
        Assert.True(resolved.AllValid, string.Join(",", resolved.Selections[0].ReasonCodes));
        Assert.Equal((false, false, ShoppingReasonCodes.SelectionFieldRequired), (rejected.AllValid, rejected.Selections[0].Status == SelectionResolutionStatus.Valid, rejected.Selections[0].ReasonCodes.Single()));
        Assert.NotNull(wifi.SelectionId);
    }

    [Fact]
    public async Task GRP_B10_the_grouped_list_reads_the_catalog_a_bounded_number_of_times()
    {
        var lab = Shop();
        var desk = await GroupedDesk.CreateAsync(lab, await AirAsync());
        var before = lab.Catalog.Reads;
        var grouped = await ListAsync(desk);
        var reads = lab.Catalog.Reads - before;
        var evaluated = grouped.Baggages.Count + grouped.Connectivity.Count;
        var direct = grouped.Connectivity.Count(item => item.SelectionId is not null);

        Assert.Equal((12, 8, 4), (evaluated, direct, grouped.PriorityServices.Count(item => item.SelectionId is null && item.RequiresDetails)));
        Assert.True(reads <= 9 + 4 * evaluated, $"grouped list took {reads} catalog reads for {evaluated} evaluated candidates");
    }
}
