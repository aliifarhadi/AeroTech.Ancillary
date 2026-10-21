using System.Text.Json;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections.Service;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.OrderDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SelectionDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2ResolveHandoffTests
{
    private const string Adult = "ADT-1";
    private const string Child = "CHD-1";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static AncillarySelection Accepted(string code)
        => code switch
        {
            "A01" => new AncillarySelection { Quantity = 2 },
            "A06" => new AncillarySelection { EquipmentKind = "BIKE", Dimensions = new SelectedDimensions(190m, 70m, 30m), WeightKg = 20m },
            "A22" => new AncillarySelection { AirportId = Trip.Ika, FacilityRef = 7001, TimeWithOffset = new DateTimeOffset(2026, 10, 16, 23, 45, 0, Trip.TehranOffset), GuestCount = 1 },
            _ => new AncillarySelection { PlanCode = "WIFI_FULL_FLIGHT", DeviceCount = 2 }
        };

    private static async Task<(ShoppingLab Lab, FixedOrders Orders, FixedClock Clock)> OrderAsync(params string[] codes)
    {
        var lab = new ShoppingLab();

        foreach (var code in codes)
        {
            var definition = lab.Definition(code);
            var provision = lab.Provision(definition, stage: PurchaseStage.Both, rules: code == "A07" ? Rules(seats: new ProvisionSeatApplicationArgs([], ["W"])) : null);

            if (provision.PriceOrigin == PriceOrigin.Filed)
                lab.Price(definition, provision, Rate(code == "A07" ? 15m : 100m));
        }

        return (lab, new FixedOrders(OrderOf(Sale.Sell(await OfferWire.ReadAsync(OfferWire.Captured())))), new FixedClock { Now = Now });
    }

    private static async Task<HandoffView> ResolveAsync(ShoppingLab lab, FixedOrders orders, FixedClock clock, SelectionLineRequest line)
    {
        var (service, _) = await BatchAsync(lab, clock: clock, orders: orders);
        var batch = await service.ExecuteAsync(ByOrder, [line], Sale);
        var references = batch.Lines.Select(row => row.PricedSelection?.Value).OfType<string>().ToList();
        var resolution = references.Count == 0 ? null : await new ResolveSelectionsService(Tokens(clock), service).ExecuteAsync(OrderDesk.OrderId, 1, references);

        return new HandoffView(batch.Basket.AllReady, resolution is null ? null : ServiceSelectionResolutionMapper.Map(resolution, null).Results.Single());
    }

    private sealed record HandoffView(bool AllReady, ServiceResolvedSelectionDto? Result);

    [Theory]
    [InlineData("A01")]
    [InlineData("A06")]
    [InlineData("A22")]
    [InlineData("A24")]
    public async Task B1_the_selection_the_customer_sent_reaches_ordering_unchanged_with_the_commercial_and_document_facts(string code)
    {
        var (lab, orders, clock) = await OrderAsync(code);
        var shopping = await ShopOrderAsync(lab, orders);
        var item = shopping.Items.First(row => row.Candidate.VariantCode == code && row.Candidate.Travellers.SequenceEqual([Adult]));
        var sent = Accepted(code);

        var view = await ResolveAsync(lab, orders, clock, new SelectionLineRequest("line", item.OfferItemRef, sent));
        var service = view.Result!.Service!;
        var candidate = item.Candidate;

        Assert.True(view.AllReady);
        Assert.Equal("Valid", view.Result.Status);
        Assert.Equal(JsonSerializer.Serialize(sent, Json), JsonSerializer.Serialize(service.Selection, Json));
        Assert.Equal(
            (code, candidate.ServiceTypeCode, candidate.ServiceSubCode, candidate.Booking.Method.ToString(), candidate.Booking.BookingRequired),
            (service.VariantCode, service.ServiceTypeCode, service.ServiceSubCode, service.BookingMethod, service.BookingRequired));
        Assert.Equal(
            (candidate.Document.Routing.ToString(), candidate.Document.DocumentType.ToString(), candidate.Document.DocumentRequired, candidate.Confirmation.ConfirmationRequirement.ToString(), candidate.Price.PricingRateId),
            (service.DocumentRouting, service.DocumentType, service.DocumentRequired, service.ConfirmationRequirement, service.PricingRateId));
        Assert.Equal(new[] { 101L }, service.OrderTravellerIds);
    }

    [Fact]
    public async Task B1_a_weight_package_is_not_offered_because_a_flight_offer_or_order_allowance_is_always_counted_in_pieces()
    {
        var (lab, orders, _) = await OrderAsync("A02");

        var shopping = await ShopOrderAsync(lab, orders);

        Assert.DoesNotContain(shopping.Items, row => row.Candidate.VariantCode == "A02");
        Assert.All(shopping.PointsOfSale.Single().Context.TravellerBaggageFacts, fact => Assert.NotNull(fact.CheckedPieces));
    }

    [Fact]
    public async Task B1_an_unverified_pet_is_never_handed_to_ordering()
    {
        var (lab, orders, clock) = await OrderAsync("A13");
        var shopping = await ShopOrderAsync(lab, orders);
        var item = shopping.Items.First(row => row.Candidate.VariantCode == "A13" && row.Candidate.Travellers.SequenceEqual([Adult]));
        var pet = new AncillarySelection
        {
            AnimalType = PetAnimalType.Cat,
            CombinedWeightKg = 5m,
            CarrierDimensions = new SelectedDimensions(50m, 30m, 20m),
            DocumentAcknowledgements = ["ENTRY_RULES_ACK"],
            AnimalAgeWeeks = 20
        };

        var view = await ResolveAsync(lab, orders, clock, new SelectionLineRequest("pet", item.OfferItemRef, pet));

        Assert.False(view.AllReady);
        Assert.Null(view.Result);
    }
}
