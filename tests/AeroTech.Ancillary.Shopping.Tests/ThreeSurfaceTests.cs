using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Api;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.SeatOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.Api;
using AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.Backoffice;
using AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.OtaPanel;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.Api;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.Backoffice;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.OtaPanel;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Api;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Backoffice;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.OtaPanel;
using AeroTech.Ancillary.RestApi.V1.AncillaryOffers.Controllers;
using SeatControllers = AeroTech.Ancillary.RestApi.V1.SeatOffers.Controllers;
using AeroTech.Ancillary.RestApi.V1._Shared;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Framework.Presentation.Json;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class ThreeSurfaceTests
{
    private const int Irr = 70;

    private static readonly DeskScope Desk = new(Airline, Pos, null, null, Now);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(), new LongToStringJsonConverter(), new NullableLongToStringJsonConverter() }
    };

    private static ShoppingLab IrrLab()
    {
        var lab = new ShoppingLab();

        lab.Catalog.CurrencyScales[Irr] = 0;

        return lab;
    }

    private static ShoppingLab Shop()
    {
        var lab = IrrLab();
        var bag = lab.Definition("A01");
        var seat = lab.Definition("A07");
        var wifi = lab.Definition("A24");

        lab.Price(bag, lab.Provision(bag), Rate(5_000_000m, Irr, components: [Tax("VAT", 450_000m, TaxTreatment.AddedToBase, Irr)]));
        lab.Price(seat, lab.Provision(seat), Rate(1_500_000m, Irr));
        lab.Price(
            wifi,
            lab.Provision(wifi),
            Rate(900_000m, Irr, components: [Tax("VAT", 81_000m, TaxTreatment.AddedToBase, Irr), Fee("TKT", 100_000m, FeeApplicationUnit.Ticket, Irr)]));

        return lab;
    }

    private static AgencySalesScopeService Agency(Caller? caller = null, Country? country = null, Eligibility? eligibility = null, FixedClock? clock = null, HomeAirline? home = null)
        => new(
            caller ?? new Caller { ContextType = CallerContextType.TravelAgency, AirlineUserId = null, AirlineOfficeId = null, TravelAgencyId = 31, TravelAgencyOfficeIds = [33, 34], CustomerId = 4040 },
            country ?? new Country { CountryId = 98 },
            home ?? new HomeAirline(),
            eligibility ?? new Eligibility { Result = InventoryReferenceCheck.Verified, Allowed = [Pos] },
            clock ?? new FixedClock());

    private static readonly Dictionary<int, string> CurrencyCodes = new() { [Irr] = "IRR", [155] = "USD" };

    private sealed record AgencyItem(
        string OfferItemRef,
        IReadOnlyList<string> SegmentIds,
        IReadOnlyList<string> TravelerIds,
        ApiElementaryPriceDto? Price,
        ApiAncillaryDto Ancillary);

    private static IEnumerable<AgencyItem> Items(ApiAncillaryOffersDto response)
        => response.Included.Bags
            .Select(entry => new AgencyItem(entry.Key, entry.Value.SegmentIds, entry.Value.TravelerIds, entry.Value.Price, entry.Value.Ancillary))
            .Concat(response.Included.OtherServices.Select(
                entry => new AgencyItem(entry.Key, entry.Value.SegmentIds, entry.Value.TravelerIds, entry.Value.Price, entry.Value.Ancillary)));

    private static ApiOfferReferences References(AncillaryShoppingSnapshot snapshot, AncillaryOfferDetailSnapshot? detail = null)
    {
        var flights = snapshot.Offer.Bounds.SelectMany(bound => bound.Flights).ToList();
        var baggage = new Dictionary<long, BaggageSpecificationArgs>();

        if (detail?.Specification.Baggage is { } specification)
            baggage[detail.Item.Candidate.DefinitionVersionId] = specification;

        return new ApiOfferReferences(
            snapshot.Offer,
            flights
                .SelectMany(flight => flight.Legs.SelectMany(leg => new[] { leg.OriginAirportId, leg.DestinationAirportId }))
                .Distinct()
                .ToDictionary(id => id, id => $"P{id:00}"),
            flights.SelectMany(flight => new[] { flight.MarketingAirlineId, flight.OperatingAirlineId }).Distinct().ToDictionary(id => id, id => $"L{id}"),
            CurrencyCodes,
            baggage);
    }

    private static IEnumerable<OtaPanelServiceDto> Services(OtaPanelAncillaryOffersDto response)
        => response.TravellerGroups.SelectMany(group => group.FlightGroups).SelectMany(group => group.Services);

    private static string[] Members(Type type) => type.GetProperties().Select(property => property.Name).Order(StringComparer.Ordinal).ToArray();

    private static async Task<(GetAncillaryOffersService Offers, AncillaryShoppingSnapshot Snapshot)> ShoppedAsync(
        ShoppingLab lab,
        AncillarySalesScope scope,
        string? wire = null)
    {
        var offers = await ServiceAsync(lab, wire);

        return (offers, await offers.ExecuteAsync(new OfferQuery(OfferWire.OfferId), scope));
    }

    [Fact]
    public async Task P32B_SRF_01_one_snapshot_gives_the_same_decision_amount_and_status_on_three_different_contracts()
    {
        var snapshot = await SnapshotAsync(Shop(), scope: Desk);
        var backoffice = BackofficeAncillaryOffersMapper.Map(snapshot);
        var api = ApiAncillaryOffersMapper.Map(snapshot, References(snapshot));
        var panel = OtaPanelAncillaryOffersMapper.Map(snapshot);
        var offered = snapshot.Items.Where(item => ApiAncillaryOffersMapper.IsOffered(item.Candidate.OfferReadiness)).ToList();

        Assert.Equal(12, snapshot.Items.Count());
        Assert.Equal(12, offered.Count);
        Assert.Equal((12, 0), (backoffice.Offers.Count, backoffice.BlockedOffers.Count));
        Assert.Equal((12, 0), (Items(api).Count(), api.Meta.WithheldCount));
        Assert.Equal((12, 0), (Services(panel).Count(), panel.BlockedCount));
        Assert.All(offered, item =>
        {
            var price = item.Candidate.Price;
            var operatorView = backoffice.Offers.Single(offer => offer.OfferItemRef == item.OfferItemRef);
            var agencyView = Items(api).Single(offer => offer.OfferItemRef == item.OfferItemRef);
            var panelView = Services(panel).Single(service => service.OfferItemRef == item.OfferItemRef);

            Assert.Equal((price.CompleteUnitTotal, price.CurrencyId), (operatorView.Pricing.CompleteUnitTotal, operatorView.Pricing.CurrencyId));
            Assert.Equal(
                (price.CompleteUnitTotal is { } unit ? ApiOfferReferences.Amount(unit) : null, price.CompleteUnitTotal is null ? null : References(snapshot).CurrencyCode(price.CurrencyId)),
                (agencyView.Price?.Amount, agencyView.Price?.CurrencyCode));
            Assert.Equal((price.CompleteUnitTotal, price.CurrencyId), (panelView.UnitPrice?.Amount, panelView.UnitPrice?.CurrencyId));
            Assert.Equal(item.Candidate.OfferReadiness.ToString(), operatorView.OfferReadiness.Name);
            Assert.Equal(item.Candidate.OfferReadiness.ToString(), agencyView.Ancillary.Readiness);
            Assert.Equal(OtaPanelAncillaryOffersMapper.DisplayStatusOf(item.Candidate.OfferReadiness), panelView.DisplayStatus);
            Assert.Equal(price.Status.ToString(), operatorView.Pricing.Status.Name);
            Assert.Equal(price.Status.ToString(), agencyView.Ancillary.Pricing.Status);
            Assert.Equal(price.Status.ToString(), panelView.PriceStatus);
            Assert.Equal(item.Candidate.FlightRefs, operatorView.Coverage.FlightRefs);
            Assert.Equal(References(snapshot).SegmentIds(item.Candidate.FlightRefs), agencyView.SegmentIds);
            Assert.Equal(References(snapshot).TravelerIds(item.Candidate.Travellers), agencyView.TravelerIds);
            Assert.Equal(item.Candidate.FlightRefs, panelView.CoveredFlightRefs);
            Assert.Equal(
                price.UnappliedFeeLines.Select(line => ApiOfferReferences.Amount(line.Amount)),
                agencyView.Ancillary.Pricing.Fees.Where(fee => fee.OrderLevel).Select(fee => fee.Amount));
            Assert.Equal(price.UnappliedFeeLines.Select(line => line.Amount), panelView.OrderLevelFees.Select(line => line.Amount));
            Assert.False(operatorView.CanPurchase || agencyView.Ancillary.CanPurchase || panelView.CanPurchase);
            Assert.Null(agencyView.Ancillary.Pricing.RequestedTotal);
        });
        Assert.Equal(3, new[] { backoffice.GetType(), api.GetType(), panel.GetType() }.Distinct().Count());
        Assert.Empty(Members(typeof(ApiAncillaryOffersDto)).Intersect(Members(typeof(BackofficeAncillaryOffersDto))));
        Assert.Equal(new[] { "Data", "Included", "Meta" }, Members(typeof(ApiAncillaryOffersDto)));
        Assert.Equal(new[] { "BlockedCount", "CurrencyCode", "CurrencyId", "OfferId", "TravellerGroups" }, Members(typeof(OtaPanelAncillaryOffersDto)));
    }

    [Fact]
    public async Task P32B_SRF_02_the_agency_api_is_normalised_and_shows_no_internal_identifier_or_diagnostic()
    {
        var lab = Shop();
        var blocked = lab.Definition("A13");

        lab.Price(blocked, lab.Provision(blocked), Rate(2_000_000m, Irr));

        var snapshot = await SnapshotAsync(lab, scope: Desk.On(AgencySalesPolicy.SalesChannel, Pos));
        var api = ApiAncillaryOffersMapper.Map(snapshot, References(snapshot));
        var json = JsonSerializer.Serialize(api, Json);
        var withheld = snapshot.Items.Where(item => !ApiAncillaryOffersMapper.IsOffered(item.Candidate.OfferReadiness)).ToList();

        Assert.NotEmpty(withheld);
        Assert.Equal(withheld.Count, api.Meta.WithheldCount);
        Assert.Equal(ApiAncillaryOffersMapper.AncillaryOffers, api.Data.Type);
        Assert.Equal(OfferWire.OfferId, api.Data.FlightOfferId);
        Assert.Equal(new[] { "1", "2" }, api.Data.Travelers.Select(traveler => traveler.Id));
        Assert.Equal(new[] { "ADULT", "CHILD" }, api.Data.Travelers.Select(traveler => traveler.TravelerType));
        Assert.Equal(Enumerable.Range(1, api.Data.Segments.Count).Select(number => number.ToString()), api.Data.Segments.Select(segment => segment.Id));
        Assert.Equal(snapshot.Offer.Bounds.Sum(bound => bound.Flights.Sum(flight => flight.Legs.Count)), api.Data.Segments.Count);
        Assert.All(api.Data.Segments, segment =>
        {
            Assert.Matches("^P[0-9]+$", segment.Departure.IataCode);
            Assert.Matches("^P[0-9]+$", segment.Arrival.IataCode);
            Assert.Matches("^L[0-9]+$", segment.CarrierCode);
        });
        Assert.Equal(3, Items(api).Select(item => item.Ancillary.Service.ServiceDefinitionRef).Distinct().Count());
        Assert.DoesNotContain(Items(api), item => item.Ancillary.Service.VariantCode == "A13");
        Assert.Equal(api.Meta.Count, Items(api).Count());
        Assert.NotEmpty(api.Included.Bags);
        Assert.All(api.Included.Bags.Values, bag => Assert.Equal(("CHECKED_BAGS", "EXTRA_CHECKED_BAG"), (bag.Ancillary.Type, bag.Name)));
        Assert.Equal(new[] { "ONBOARD_WI_FI", "STANDARD_SEAT" }, api.Included.OtherServices.Values.Select(service => service.Name).Distinct().Order());
        Assert.Equal(new[] { "OTHER_SERVICES", "SEATS" }, api.Included.OtherServices.Values.Select(service => service.Ancillary.Type).Distinct().Order());
        Assert.All(Items(api), item =>
        {
            Assert.NotEmpty(item.TravelerIds);
            Assert.NotEmpty(item.SegmentIds);
            Assert.All(item.TravelerIds, id => Assert.Contains(api.Data.Travelers, traveler => traveler.Id == id));
            Assert.All(item.SegmentIds, id => Assert.Contains(api.Data.Segments, segment => segment.Id == id));
            Assert.False(item.Ancillary.Availability.Guaranteed);
            Assert.NotEqual("NeedsVerification", item.Ancillary.Readiness);
            Assert.True(item.Price is null || item.Price.CurrencyCode == "IRR");
        });
        Assert.Equal(ApiAncillaryOffersMapper.NeedsSelectionOrRecheck, api.Meta.Status);
        Assert.All(
            new[] { "provisionId", "pricingRevisionId", "pricingRateId", "definitionVersionId", "pointOfSale", "supplierId", "diagnostics", "missingContextFields", "ownerAirlineId" },
            member => Assert.DoesNotContain(member, json, StringComparison.OrdinalIgnoreCase));
        Assert.All(
            snapshot.Items.SelectMany(item => new[] { item.Candidate.ProvisionId, item.Candidate.DefinitionVersionId, item.Candidate.Price.PricingRevisionId ?? 0, item.PointOfSaleId }).Where(id => id > 999).Distinct(),
            id => Assert.DoesNotContain($"\"{id}\"", json, StringComparison.Ordinal));
        Assert.Contains("\"price\": {", json, StringComparison.Ordinal);
        Assert.Contains("\"other-services\": {", json, StringComparison.Ordinal);
        Assert.Contains("\"currencyCode\": \"IRR\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("currencyId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("airportId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("airlineId", json, StringComparison.OrdinalIgnoreCase);
        Assert.All(new[] { "ADT-1", "CHD-1", "B1:F1" }, reference => Assert.DoesNotContain(reference, json, StringComparison.Ordinal));
        Assert.Contains("\"readiness\": \"NeedsSelection\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task P32B_SRF_03_the_panel_groups_each_service_once_under_its_traveller_and_first_covered_flight()
    {
        var snapshot = await SnapshotAsync(Shop(), scope: Desk.On(AgencySalesPolicy.SalesChannel, Pos));
        var panel = OtaPanelAncillaryOffersMapper.Map(snapshot);

        Assert.Equal(new[] { "ADT-1", "CHD-1" }, panel.TravellerGroups.Select(group => group.TravellerRef));
        Assert.All(panel.TravellerGroups, group =>
        {
            Assert.Equal(new[] { "B1:F1", "B2:F1" }, group.FlightGroups.Select(flight => flight.FlightRef));
            Assert.All(group.FlightGroups, flight =>
            {
                Assert.Equal((1, 25m, "Kg"), (flight.IncludedCheckedBaggage!.Pieces, flight.IncludedCheckedBaggage.Weight, flight.IncludedCheckedBaggage.WeightUnit));
                Assert.Equal(new[] { "Baggage", "Seat", "Connectivity" }, flight.Services.Select(service => service.Category));
                Assert.All(flight.Services, service => Assert.Equal(flight.FlightRef, service.CoveredFlightRefs[0]));
            });
        });
        Assert.Equal(Services(panel).Count(), Services(panel).Select(service => service.OfferItemRef).Distinct().Count());
        Assert.All(Services(panel), service => Assert.True(service.RequiresRecheck));
        Assert.Equal((Irr, "IRR"), (panel.CurrencyId, panel.CurrencyCode));
    }

    [Fact]
    public async Task P32B_POS_01_every_authorized_point_of_sale_is_shopped_and_its_offers_stay_apart()
    {
        var lab = IrrLab();
        var wifi = lab.Definition("A24");

        lab.Price(wifi, lab.Provision(wifi, pos: Pos), Rate(900_000m, Irr));
        lab.Price(wifi, lab.Provision(wifi, sequence: 20, pos: OtherPos), Rate(700_000m, Irr));

        var (offers, both) = await ShoppedAsync(lab, Desk.On(Channel.BackOffice, OtherPos, Pos, OtherPos));
        var one = await offers.ExecuteAsync(new OfferQuery(OfferWire.OfferId), Desk.On(Channel.BackOffice, OtherPos));
        var none = await offers.ExecuteAsync(new OfferQuery(OfferWire.OfferId), Desk.On(Channel.BackOffice));
        var unknown = await offers.ExecuteAsync(new OfferQuery(OfferWire.OfferId), Desk.On(Channel.BackOffice, 4242));
        var backoffice = BackofficeAncillaryOffersMapper.Map(both);
        var api = ApiAncillaryOffersMapper.Map(both, References(both));

        Assert.Equal(new[] { Pos, OtherPos }, both.PointsOfSale.Select(pointOfSale => pointOfSale.PointOfSaleId));
        Assert.Equal(8, both.Items.Count());
        Assert.Equal(8, both.Items.Select(item => item.OfferItemRef).Distinct().Count());
        Assert.All(both.PointsOfSale, pointOfSale => Assert.All(pointOfSale.Items, item =>
        {
            Assert.Equal(pointOfSale.PointOfSaleId, item.Context.PointOfSaleId);
            Assert.Equal(pointOfSale.PointOfSaleId == Pos ? 900_000m : 700_000m, item.Candidate.Price.CompleteUnitTotal);
        }));
        Assert.Null(backoffice.Scope.PointOfSaleId);
        Assert.Equal(new[] { Pos, OtherPos }, backoffice.Scope.PointOfSaleIds);
        Assert.Equal(new[] { 4, 4 }, new[] { Pos, OtherPos }.Select(pointOfSale => backoffice.Offers.Count(offer => offer.PointOfSaleId == pointOfSale)));
        Assert.Equal(8, Items(api).Count());
        Assert.Equal(new[] { "900000.00", "700000.00" }, Items(api).Select(item => item.Price!.Amount).Distinct());
        Assert.Single(Items(api).Select(item => item.Ancillary.Service.ServiceDefinitionRef).Distinct());
        Assert.Equal((OtherPos, 4, 700_000m), (one.PointsOfSale.Single().PointOfSaleId, one.Items.Count(), one.Items.Select(item => item.Candidate.Price.CompleteUnitTotal).Distinct().Single()));
        Assert.Empty(none.PointsOfSale);
        Assert.Equal(BackofficeAncillaryOffersMapper.NoAuthorizedPointOfSale, Assert.Single(BackofficeAncillaryOffersMapper.Map(none).Diagnostics).Code);
        Assert.Equal((ApiAncillaryOffersMapper.NoApplicableOffer, 0), (ApiAncillaryOffersMapper.Map(none, References(none)).Meta.Status, ApiAncillaryOffersMapper.Map(none, References(none)).Meta.Count));
        Assert.Equal(2, OtaPanelAncillaryOffersMapper.Map(none).TravellerGroups.Count);
        Assert.Empty(unknown.Items);
        Assert.Equal(BackofficeAncillaryOffersMapper.NoApplicableProduct, Assert.Single(BackofficeAncillaryOffersMapper.Map(unknown).Diagnostics).Code);
    }

    [Fact]
    public async Task P32B_AGY_01_the_agency_scope_is_the_agency_claims_the_country_header_the_api_channel_currency_70_and_the_clock()
    {
        var eligibility = new Eligibility { Result = InventoryReferenceCheck.Verified, Allowed = [6, 5, 6] };
        var clock = new FixedClock();
        var scope = await Agency(eligibility: eligibility, clock: clock).ResolveAsync();
        var noOffice = new Eligibility { Result = InventoryReferenceCheck.Verified };
        var officeless = await Agency(
            new Caller { ContextType = CallerContextType.TravelAgency, AirlineUserId = null, AirlineOfficeId = null, TravelAgencyId = 31 },
            new Country(),
            noOffice).ResolveAsync();

        Assert.Equal(
            (Airline, Channel.PartnerAPI, (long?)4040, (int?)AgencySalesPolicy.CurrencyId, clock.Now),
            (scope.OwnerAirlineId, scope.Channel, scope.CustomerId, scope.PreferredCurrencyId, scope.SalesInstantUtc));
        Assert.Equal(new long[] { 5, 6 }, scope.PointOfSaleIds);
        Assert.Equal((70, "PartnerAPI"), (AgencySalesPolicy.CurrencyId, AgencySalesPolicy.SalesChannel.ToString()));
        Assert.Equal(
            (Channel.PartnerAPI, (long?)98, 0, (long?)33),
            (eligibility.Asked!.Channel, eligibility.Asked.CountryId, eligibility.Asked.AirlineOfficeIds.Count, eligibility.Asked.TravelAgencyOfficeId));
        Assert.Equal(((long?)null, (long?)0), (noOffice.Asked!.CountryId, noOffice.Asked.TravelAgencyOfficeId));
        Assert.Empty(officeless.PointOfSaleIds);

        var noAgency = await Assert.ThrowsAsync<BusinessException>(() => Agency(new Caller { TravelAgencyId = null }).ResolveAsync());
        var noHome = await Assert.ThrowsAsync<BusinessException>(() => Agency(home: new HomeAirline { AirlineId = null }).ResolveAsync());
        var notConnected = await Assert.ThrowsAsync<BusinessException>(() => Agency(eligibility: new Eligibility()).ResolveAsync());

        Assert.Equal((16809, 403), (noAgency.Code, noAgency.HttpStatus));
        Assert.Equal((16809, 403), (noHome.Code, noHome.HttpStatus));
        Assert.Equal((16808, 409), (notConnected.Code, notConnected.HttpStatus));
        Assert.Contains(AuthorizedPointsOfSale.BlockedExternalReference, notConnected.Message, StringComparison.Ordinal);

        var apiScope = await new ApiSalesScopeResolver(Agency(eligibility: new Eligibility { Result = InventoryReferenceCheck.Verified, Allowed = [5] })).ResolveAsync();
        var panelScope = await new OtaPanelSalesScopeResolver(Agency(eligibility: new Eligibility { Result = InventoryReferenceCheck.Verified, Allowed = [5] })).ResolveAsync();

        Assert.Equal(DeskScope.Of(apiScope), DeskScope.Of(panelScope));
        Assert.Equal(apiScope.Channel, panelScope.Channel);
    }

    [Fact]
    public void P32B_AGY_02_the_agency_requests_carry_the_offer_id_only_and_forged_backoffice_fields_change_nothing()
    {
        const string Forged = """{"offerId":"X","options":{"pointOfSaleId":77,"customerId":"501","preferredCurrencyId":47},"pointOfSaleId":77,"customerId":"501","preferredCurrencyId":47}""";

        Assert.Equal(new[] { "OfferId" }, Members(typeof(ApiGetAncillaryOffersQuery)));
        Assert.Equal(new[] { "OfferId" }, Members(typeof(OtaPanelGetAncillaryOffersQuery)));
        Assert.Equal(new[] { "OfferId", "Options" }, Members(typeof(BackofficeGetAncillaryOffersQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef" }, Members(typeof(ApiGetAncillaryOfferDetailQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef" }, Members(typeof(OtaPanelGetAncillaryOfferDetailQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef", "Options" }, Members(typeof(BackofficeGetAncillaryOfferDetailQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef", "Selection" }, Members(typeof(ApiEvaluateSelectionQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef", "Selection" }, Members(typeof(OtaPanelEvaluateSelectionQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef", "Options", "Selection" }, Members(typeof(BackofficeEvaluateSelectionQuery)));
        Assert.Equal(new[] { "OfferId" }, Members(typeof(ApiGetSeatOffersQuery)));
        Assert.Equal(new[] { "OfferId" }, Members(typeof(OtaPanelGetSeatOffersQuery)));
        Assert.Equal(new[] { "OfferId", "Options" }, Members(typeof(BackofficeGetSeatOffersQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef" }, Members(typeof(ApiGetSeatOfferDetailQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef" }, Members(typeof(OtaPanelGetSeatOfferDetailQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef", "Options" }, Members(typeof(BackofficeGetSeatOfferDetailQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef", "Selection" }, Members(typeof(ApiEvaluateSeatSelectionQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef", "Selection" }, Members(typeof(OtaPanelEvaluateSeatSelectionQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef", "Options", "Selection" }, Members(typeof(BackofficeEvaluateSeatSelectionQuery)));
        Assert.Equal("X", JsonSerializer.Deserialize<ApiGetAncillaryOffersQuery>(Forged, Json)!.OfferId);
        Assert.Equal("X", JsonSerializer.Deserialize<OtaPanelGetAncillaryOffersQuery>(Forged, Json)!.OfferId);
        Assert.Equal(77, JsonSerializer.Deserialize<BackofficeGetAncillaryOffersQuery>(Forged, Json)!.Options!.PointOfSaleId);
        Assert.Equal(
            new[] { typeof(ICallerContext), typeof(IRequestCountry), typeof(IOperatorAirlineResolver), typeof(IPointOfSaleEligibilityReference), typeof(AeroTech.Framework.Core.ServiceContracts.IClock) },
            typeof(AgencySalesScopeService).GetConstructors().Single().GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(new[] { typeof(IAgencySalesScopeService) }, typeof(ApiSalesScopeResolver).GetConstructors().Single().GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(new[] { typeof(IAgencySalesScopeService) }, typeof(OtaPanelSalesScopeResolver).GetConstructors().Single().GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            37,
            new[] { typeof(BackofficeAncillarySelectionInput), typeof(ApiAncillarySelectionInput), typeof(OtaPanelAncillarySelectionInput) }
                .Select(type => Members(type).SequenceEqual(Members(typeof(AncillarySelection))) ? Members(type).Length : -1)
                .Distinct()
                .Single());
    }

    [Fact]
    public void P32B_SRF_05_three_controllers_expose_the_same_three_operations_on_three_routes_under_three_policies()
    {
        var controllers = new[] { typeof(BackofficeController), typeof(OtaController), typeof(OtaPanelController) }
            .Select(type => (
                Route: type.GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Single().Template,
                Policy: type.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Policy,
                Actions: type.GetMethods()
                    .SelectMany(method => method.GetCustomAttributes(typeof(HttpPostAttribute), false).Cast<HttpPostAttribute>().Select(post => (post.Template ?? string.Empty, method.GetParameters()[0].ParameterType)))
                    .OrderBy(action => action.Item1, StringComparer.Ordinal)
                    .ToList()))
            .ToList();

        Assert.Equal(
            new[]
            {
                ("Backoffice/v{version:apiVersion}/AncillaryOffers", SurfaceAuthorization.Backoffice),
                ("Api/v{version:apiVersion}/AncillaryOffers", SurfaceAuthorization.Ota),
                ("OtaPanel/v{version:apiVersion}/AncillaryOffers", SurfaceAuthorization.OtaPanel)
            },
            controllers.Select(controller => (controller.Route, controller.Policy!)));
        Assert.All(controllers, controller => Assert.Equal(new[] { "", "Details", "Selections/Evaluate" }, controller.Actions.Select(action => action.Item1)));
        Assert.Equal(9, controllers.SelectMany(controller => controller.Actions.Select(action => action.Item2)).Distinct().Count());
        Assert.Equal(3, new[] { SurfaceAuthorization.Backoffice, SurfaceAuthorization.Ota, SurfaceAuthorization.OtaPanel }.Distinct().Count());
    }

    [Fact]
    public async Task P32B_SRF_04_equal_commercial_facts_give_equal_candidates_and_amounts_on_every_surface()
    {
        var lab = Shop();
        var backoffice = await SnapshotAsync(lab, scope: new DeskScope(Airline, Pos, 4040, Irr, Now));
        var agency = await SnapshotAsync(lab, scope: new DeskScope(Airline, Pos, 4040, Irr, Now).On(AgencySalesPolicy.SalesChannel, Pos));

        Assert.Equal(
            backoffice.Items.Select(item => (item.Candidate.CandidateIdentity, item.Candidate.OfferReadiness, item.Candidate.Price.CompleteUnitTotal, item.Candidate.Price.CurrencyId)),
            agency.Items.Select(item => (item.Candidate.CandidateIdentity, item.Candidate.OfferReadiness, item.Candidate.Price.CompleteUnitTotal, item.Candidate.Price.CurrencyId)));
        Assert.Empty(backoffice.Items.Select(item => item.OfferItemRef).Intersect(agency.Items.Select(item => item.OfferItemRef)));
    }

    [Fact]
    public async Task P32B_REF_01_an_offer_item_ref_is_opaque_and_is_found_again_only_by_shopping_the_same_offer_in_the_same_scope()
    {
        var lab = Shop();
        var (offers, snapshot) = await ShoppedAsync(lab, Desk);
        var item = snapshot.Items.First(candidate => candidate.Candidate.VariantCode == "A01");
        var (_, found) = await offers.FindItemAsync(new OfferQuery(OfferWire.OfferId, item.OfferItemRef), Desk);

        Assert.Equal(43, item.OfferItemRef.Length);
        Assert.Matches("^[A-Za-z0-9_-]+$", item.OfferItemRef);
        Assert.All(
            new[] { item.Candidate.ProvisionId, item.Candidate.DefinitionVersionId, item.Candidate.Price.PricingRevisionId!.Value },
            id => Assert.DoesNotContain(id.ToString(), item.OfferItemRef, StringComparison.Ordinal));
        Assert.Equal(item.Candidate.CandidateIdentity, found.Candidate.CandidateIdentity);
        Assert.Equal(item.OfferItemRef, OfferItemRef.Of(OfferWire.OfferId, Desk, Pos, item.Candidate.CandidateIdentity));
        Assert.NotEqual(item.OfferItemRef, OfferItemRef.Of(OfferWire.OfferId, Desk.On(AgencySalesPolicy.SalesChannel, Pos), Pos, item.Candidate.CandidateIdentity));
        Assert.NotEqual(item.OfferItemRef, OfferItemRef.Of(OfferWire.OfferId, Desk, OtherPos, item.Candidate.CandidateIdentity));
        Assert.NotEqual(item.OfferItemRef, OfferItemRef.Of(OfferWire.OfferId + "x", Desk, Pos, item.Candidate.CandidateIdentity));

        foreach (var (query, scope) in new (OfferQuery Query, AncillarySalesScope Scope)[]
                 {
                     (new OfferQuery(OfferWire.OfferId, item.OfferItemRef), Desk.On(AgencySalesPolicy.SalesChannel, Pos)),
                     (new OfferQuery(OfferWire.OfferId, item.OfferItemRef), Desk.On(Channel.BackOffice, OtherPos)),
                     (new OfferQuery(OfferWire.OfferId, item.OfferItemRef), Desk with { OwnerAirlineId = 2 }),
                     (new OfferQuery(OfferWire.OfferId, item.Candidate.ProvisionId.ToString()), Desk),
                     (new OfferQuery(OfferWire.OfferId, item.OfferItemRef[..42] + "A"), Desk)
                 })
        {
            var missing = await Assert.ThrowsAsync<BusinessException>(() => offers.FindItemAsync(query, scope));

            Assert.Equal((16811, 404), (missing.Code, missing.HttpStatus));
            Assert.StartsWith("OFFER_ITEM_NOT_FOUND", missing.Message, StringComparison.Ordinal);
        }

        var blank = await Assert.ThrowsAsync<BusinessException>(() => offers.FindItemAsync(new OfferQuery(OfferWire.OfferId, " "), Desk));

        Assert.Equal((16810, 422), (blank.Code, blank.HttpStatus));

        var bag = lab.Catalog.Definitions.Single(definition => definition.VariantCode == "A01");
        var provision = lab.Catalog.Provisions.Single(row => row.ServiceDefinitionId == bag.Id);

        lab.Catalog.Pricings.RemoveAll(pricing => pricing.AncillaryProvisionId == provision.Id);
        lab.Price(bag, provision, Rate(6_000_000m, Irr));

        var stale = await Assert.ThrowsAsync<BusinessException>(() => offers.FindItemAsync(new OfferQuery(OfferWire.OfferId, item.OfferItemRef), Desk));

        Assert.Equal((16811, 404), (stale.Code, stale.HttpStatus));
    }

    [Theory]
    [InlineData("A01", "ExtraPiece")]
    [InlineData("A03", "Overweight")]
    [InlineData("A04", "Oversize")]
    [InlineData("A05", "ExtraPiece")]
    [InlineData("A06", "SpecialEquipment")]
    public async Task P32B_DET_01_a_baggage_detail_is_the_published_specification_the_provision_rule_the_source_allowance_and_the_engine_price(string code, string chargeKind)
    {
        var lab = IrrLab();
        var definition = lab.Definition(code);

        lab.Price(definition, lab.Provision(definition), Rate(5_000_000m, Irr, components: [Tax("VAT", 450_000m, TaxTreatment.AddedToBase, Irr)]));

        var (offers, snapshot) = await ShoppedAsync(lab, Desk);
        var item = snapshot.Items.First();
        var details = new GetAncillaryOfferDetailService(offers, lab.Catalog, lab.Catalog, lab.Catalog);
        var detail = await details.ExecuteAsync(new OfferQuery(OfferWire.OfferId, item.OfferItemRef), Desk);
        var specification = definition.SpecificationArgs().Baggage!;
        var allowanceConcept = specification.AllowanceConcept?.ToString();
        var backoffice = BackofficeAncillaryOfferDetailMapper.Map(detail);
        var api = ApiAncillaryOfferDetailMapper.Map(detail, References(detail.Shopping, detail)).Data;
        var panel = OtaPanelAncillaryOfferDetailMapper.Map(detail);

        Assert.Equal(specification, detail.Specification.Baggage);
        Assert.Equal(chargeKind, specification.ChargeKind.ToString());
        Assert.Equal(
            (chargeKind, allowanceConcept, specification.PackageWeightKg, specification.MaxKgPerPiece, specification.WeightFromExclusiveKg, specification.WeightToInclusiveKg, specification.MaxLinearSumCm, specification.EquipmentKind),
            (api.Specification.Baggage!.ChargeKind, api.Specification.Baggage.AllowanceConcept, api.Specification.Baggage.PackageWeightKg, api.Specification.Baggage.MaxKgPerPiece, api.Specification.Baggage.WeightFromExclusiveKg, api.Specification.Baggage.WeightToInclusiveKg, api.Specification.Baggage.MaxLinearSumCm, api.Specification.Baggage.EquipmentKind));
        Assert.Equal((chargeKind, specification.MaxKgPerPiece), (backoffice.Specification.Baggage!.ChargeKind.Name, backoffice.Specification.Baggage.MaxKgPerPiece));
        Assert.Equal((chargeKind, specification.MaxKgPerPiece), (panel.Specification.Baggage!.ChargeKind, panel.Specification.Baggage.MaxKgPerPiece));
        Assert.Equal(specification.MaxSize?.LengthCm, api.Specification.Baggage.MaxSize?.LengthCm);
        Assert.Null(api.Specification.Seat);
        Assert.NotNull(detail.BaggageRule);
        Assert.Equal((chargeKind, "Prepaid"), (api.BaggageRule!.ChargeKind, api.BaggageRule.PurchaseApplication));
        Assert.Equal(detail.BaggageRule!.Weight, api.BaggageRule.Weight);
        Assert.NotEmpty(detail.IncludedAllowance);
        Assert.All(api.IncludedAllowance, allowance =>
        {
            Assert.Equal(
                (References(detail.Shopping).TravelerIds(item.Candidate.Travellers).Single(), 25m, "Kg", true),
                (allowance.TravelerId, allowance.CheckedWeight, allowance.CheckedWeightUnit, allowance.Verified));
            Assert.Equal(1, allowance.CheckedPieces);
            Assert.NotEmpty(allowance.SegmentIds);
            Assert.All(allowance.SegmentIds, id => Assert.Contains(id, References(detail.Shopping).SegmentIds(item.Candidate.FlightRefs)));
        });
        Assert.Equal(api.IncludedAllowance.Count, panel.IncludedAllowance.Count);
        Assert.Equal(api.IncludedAllowance.Count, backoffice.IncludedAllowance.Count);
        Assert.Equal(
            (ApiOfferReferences.Amount(item.Candidate.Price.CompleteUnitTotal!.Value), "5000000.00", "450000.00", "IRR"),
            (api.Bag!.Price!.Amount, api.Bag.Ancillary.Pricing.Base, api.Bag.Ancillary.Pricing.Taxes.Single().Amount, api.Bag.Price.CurrencyCode));
        Assert.Equal(
            (specification.ChargeKind == BaggageChargeKind.WeightPackage ? (int?)null : 1, specification.PackageWeightKg ?? specification.WeightToInclusiveKg ?? specification.MaxKgPerPiece),
            (api.Bag.Quantity, api.Bag.Weight));
        Assert.Equal(api.Bag.Weight is null ? null : "KG", api.Bag.WeightUnit);
        Assert.Null(api.OtherService);
        Assert.Equal((ApiAncillaryOfferDetailMapper.AncillaryOffer, item.OfferItemRef), (api.Type, api.Id));
        Assert.Equal((5_450_000m, 5_000_000m, 450_000m), (panel.PriceBreakdown.UnitPrice!.Amount, panel.PriceBreakdown.Base!.Amount, panel.PriceBreakdown.AddedTaxes.Single().Amount));
        Assert.Equal((5_450_000m, 5_000_000m), (backoffice.Offer.Pricing.CompleteUnitTotal, backoffice.Offer.Pricing.BaseAmount));
        Assert.Equal(("5000000.00", "5450000.00", "IRR", true, true), (api.Rates.Single().Base, api.Rates.Single().Total, api.Rates.Single().CurrencyCode, api.Rates.Single().UnitTotalComplete, api.Rates.Single().Applied));
        Assert.Equal(
            (item.Candidate.Quantity.Unit.ToString(), item.Candidate.Quantity.MinPerSelection, item.Candidate.Quantity.MaxPerSelection),
            (api.Bag.Ancillary.Quantity.Unit, api.Bag.Ancillary.Quantity.Min, api.Bag.Ancillary.Quantity.Max));
        Assert.Equal(item.Candidate.Selection.Fields.Select(field => field.Name), api.Bag.Ancillary.SelectionRequirements.Select(field => field.Name));
        Assert.Equal(("Paid", "NonRefundable", definition.Booking.ConfirmationRequirement.ToString()), (api.Terms.Disposition, api.Terms.ReissueRefund, api.Terms.ConfirmationRequirement));
        Assert.Equal(("NonRefundable", false, false), (backoffice.Terms.ReissueRefund.Name, backoffice.Terms.Commissionable, backoffice.Terms.InterlineSettlement));
        Assert.Equal("Both", api.Restrictions.PurchaseStage);
        Assert.Equal(item.Candidate.OfferReadiness.ToString(), api.Bag.Ancillary.Readiness);
        Assert.False(api.Bag.Ancillary.CanPurchase || panel.Service.CanPurchase || backoffice.Offer.CanPurchase);
        Assert.False(api.Bag.Ancillary.Availability.Guaranteed);
    }

    [Fact]
    public async Task P32B_DET_04_a_weight_package_is_not_offered_from_a_source_that_states_pieces_and_weight_without_an_allowance_concept()
    {
        var lab = IrrLab();
        var package = lab.Definition("A02");

        lab.Price(package, lab.Provision(package), Rate(800_000m, Irr));

        var (_, snapshot) = await ShoppedAsync(lab, Desk);
        var context = ContextOf(snapshot.Offer, Desk, new Dictionary<int, int>());

        Assert.Equal(BaggageAllowanceConcept.Weight, package.SpecificationArgs().Baggage!.AllowanceConcept);
        Assert.All(context.TravellerBaggageFacts, fact => Assert.Equal((1, 25m), (fact.CheckedPieces, fact.CheckedWeight)));
        Assert.Empty(snapshot.Items);
        Assert.Equal(ApiAncillaryOffersMapper.NoApplicableOffer, ApiAncillaryOffersMapper.Map(snapshot, References(snapshot)).Meta.Status);
    }

    [Theory]
    [InlineData("A07")]
    [InlineData("A08")]
    [InlineData("A09")]
    public async Task P32B_DET_02_a_seat_detail_states_the_published_seat_rules_and_never_a_seat_map(string code)
    {
        var lab = IrrLab();
        var definition = lab.Definition(code);

        lab.Price(definition, lab.Provision(definition), Rate(1_500_000m, Irr));

        var (offers, snapshot) = await ShoppedAsync(lab, Desk);
        var item = snapshot.Items.First();
        var query = new OfferQuery(OfferWire.OfferId, item.OfferItemRef);
        var detail = await new GetAncillaryOfferDetailService(offers, lab.Catalog, lab.Catalog, lab.Catalog).ExecuteAsync(query, Desk);
        var seat = definition.SpecificationArgs().Seat!;
        var api = ApiAncillaryOfferDetailMapper.Map(detail, References(detail.Shopping, detail)).Data;
        var panel = OtaPanelAncillaryOfferDetailMapper.Map(detail);
        var backoffice = BackofficeAncillaryOfferDetailMapper.Map(detail);

        Assert.Equal(
            (seat.SeatPurpose.ToString(), seat.RequiresExitRowEligibility, seat.RequiresAdjacentSeat, seat.ExtraSeatPurpose?.ToString(), seat.ExtraOccupiedSeatCount, seat.RequiresExternalTicketAction),
            (api.Specification.Seat!.SeatPurpose, api.Specification.Seat.RequiresExitRowEligibility, api.Specification.Seat.RequiresAdjacentSeat, api.Specification.Seat.ExtraSeatPurpose, api.Specification.Seat.ExtraOccupiedSeatCount, api.Specification.Seat.RequiresExternalTicketAction));
        Assert.Equal(seat.SeatCharacteristicCodes, api.Specification.Seat.SeatCharacteristicCodes);
        Assert.Equal(seat.SeatCharacteristicCodes, panel.Specification.Seat!.SeatCharacteristicCodes);
        Assert.Equal(seat.SeatPurpose.ToString(), backoffice.Specification.Seat!.SeatPurpose.Name);
        Assert.Equal(new[] { "E" }, api.SeatRule!.SeatCharacteristicCodes);
        Assert.Empty(api.SeatRule.SeatNumbers);
        Assert.Null(api.BaggageRule);
        Assert.Null(api.Specification.Baggage);
        Assert.DoesNotContain(detail.IncludedAllowance, fact => !item.Candidate.FlightRefs.Contains(fact.FlightRef));
        Assert.All(new[] { "\"rows\":", "\"seats\":", "\"seatMap\":", "\"columns\":" }, member => Assert.DoesNotContain(member, JsonSerializer.Serialize(api, Json), StringComparison.OrdinalIgnoreCase));
        Assert.False(api.OtherService!.Ancillary.Availability.Guaranteed);
        Assert.Null(api.Bag);
        Assert.Equal("NotChecked", api.OtherService.Ancillary.Availability.State);
        Assert.Equal("NotApplicable", backoffice.Offer.Availability.ConfigState.Name);
        Assert.Null(backoffice.Offer.Availability.Authority);
    }

    [Fact]
    public async Task P32B_DET_03_other_profiles_show_only_their_own_published_section()
    {
        var lab = IrrLab();

        foreach (var code in new[] { "A11", "A13", "A15", "A23", "A24" })
        {
            var definition = lab.Definition(code);
            var provision = lab.Provision(definition);

            if (provision.PriceOrigin == PriceOrigin.Filed)
                lab.Price(definition, provision, Rate(1_000_000m, Irr));
        }

        var (offers, snapshot) = await ShoppedAsync(lab, Desk);
        var details = new GetAncillaryOfferDetailService(offers, lab.Catalog, lab.Catalog, lab.Catalog);
        var sections = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var item in snapshot.Items.DistinctBy(item => item.Candidate.VariantCode))
        {
            var detail = await details.ExecuteAsync(new OfferQuery(OfferWire.OfferId, item.OfferItemRef), Desk);
            var api = ApiAncillaryOfferDetailMapper.Map(detail, References(detail.Shopping, detail)).Data.Specification;
            var panel = OtaPanelAncillaryOfferDetailMapper.Map(detail).Specification;
            var backoffice = BackofficeAncillaryOfferDetailMapper.Map(detail).Specification;
            var present = typeof(ApiServiceSpecificationDto).GetProperties().Where(property => property.GetValue(api) is not null).Select(property => property.Name).ToList();

            sections[item.Candidate.VariantCode] = Assert.Single(present);
            Assert.Equal(present, typeof(OtaPanelServiceSpecificationDto).GetProperties().Where(property => property.GetValue(panel) is not null).Select(property => property.Name));
            Assert.Equal(present, backoffice.GetType().GetProperties().Where(property => property.GetValue(backoffice) is not null).Select(property => property.Name));
            Assert.Equal(item.Candidate.Profile.ToString(), present[0]);
        }

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["A11"] = "Meal", ["A13"] = "Pet", ["A15"] = "AssistedTravel", ["A23"] = "Priority", ["A24"] = "Connectivity"
            },
            sections);
    }

    [Fact]
    public async Task P32B_SEL_01_a_selection_is_evaluated_by_the_engine_and_answered_on_each_surface_without_any_hold_or_guarantee()
    {
        var lab = Shop();
        var (offers, snapshot) = await ShoppedAsync(lab, Desk);
        var service = new EvaluateSelectionService(offers, lab.Engine);
        var bag = snapshot.Items.First(item => item.Candidate.VariantCode == "A01");
        var wifi = snapshot.Items.First(item => item.Candidate.VariantCode == "A24");
        var query = new OfferQuery(OfferWire.OfferId, bag.OfferItemRef);
        var two = await service.ExecuteAsync(query, ApiSelectionInputMapper.ToSelection(new ApiAncillarySelectionInput { Quantity = 2 }), Desk);
        var nine = await service.ExecuteAsync(query, OtaPanelSelectionInputMapper.ToSelection(new OtaPanelAncillarySelectionInput { Quantity = 9 }), Desk);
        var none = await service.ExecuteAsync(query, BackofficeSelectionInputMapper.ToSelection(null), Desk);
        var forged = await service.ExecuteAsync(query, new AncillarySelection { Quantity = 2, SeatNumber = "12A" }, Desk);
        var withFee = await service.ExecuteAsync(new OfferQuery(OfferWire.OfferId, wifi.OfferItemRef), new AncillarySelection { OptIn = true }, Desk);
        var api = ApiSelectionResultMapper.Map(two, References(two.Shopping)).Data;
        var panel = OtaPanelSelectionResultMapper.Map(two);
        var backoffice = BackofficeSelectionResultMapper.Map(two);

        Assert.Equal((SelectionEvaluationStatus.Accepted, 10_900_000m, false), (two.Evaluation.Status, two.Evaluation.Candidate.Price.RequestedQuantityTotal, two.Evaluation.ProvisionChangedBySelection));
        Assert.Equal(("Accepted", "10900000.00", "IRR", "5450000.00"), (api.Status, api.SelectedPrice!.Amount, api.SelectedPrice.CurrencyCode, api.Bag!.Price!.Amount));
        Assert.Equal(ApiSelectionResultMapper.AncillarySelection, api.Type);
        Assert.Equal(("Accepted", 10_900_000m, 5_450_000m), (panel.Status, panel.SelectedAmount!.Amount, panel.Service.UnitPrice!.Amount));
        Assert.Equal(("Accepted", 10_900_000m, 5_450_000m), (backoffice.Status.Name, backoffice.SelectedQuantityTotal, backoffice.Offer.Pricing.CompleteUnitTotal));
        Assert.Equal(bag.OfferItemRef, api.OfferItemRef);
        Assert.Equal(bag.OfferItemRef, api.CurrentOfferItemRef);
        Assert.False(api.CanPurchase || panel.CanPurchase || backoffice.CanPurchase);
        Assert.False(api.Bag.Ancillary.Availability.Guaranteed);
        Assert.Null(bag.Candidate.Price.RequestedQuantityTotal);
        Assert.Equal((SelectionEvaluationStatus.Rejected, null), (nine.Evaluation.Status, nine.Evaluation.Candidate.Price.RequestedQuantityTotal));
        Assert.Null(OtaPanelSelectionResultMapper.Map(nine).SelectedAmount);
        Assert.Equal(("Quantity", ShoppingReasonCodes.QuantityAboveMaximum), (OtaPanelSelectionResultMapper.Map(nine).InputIssues.Single().Field, OtaPanelSelectionResultMapper.Map(nine).InputIssues.Single().Reason));
        Assert.Equal((SelectionEvaluationStatus.Incomplete, null), (none.Evaluation.Status, none.Evaluation.Candidate.Price.RequestedQuantityTotal));
        Assert.Null(BackofficeSelectionResultMapper.Map(none).SelectedQuantityTotal);
        Assert.Equal(SelectionEvaluationStatus.Rejected, forged.Evaluation.Status);
        Assert.Contains(ApiSelectionResultMapper.Map(forged, References(forged.Shopping)).Data.Issues, issue => issue is { Field: "SeatNumber", Reason: ShoppingReasonCodes.SelectionFieldNotAllowed });
        Assert.Equal((981_000m, null), (withFee.Evaluation.Candidate.Price.CompleteUnitTotal, withFee.Evaluation.Candidate.Price.RequestedQuantityTotal));
        Assert.Null(ApiSelectionResultMapper.Map(withFee, References(withFee.Shopping)).Data.SelectedPrice);
        Assert.Equal("100000.00", ApiSelectionResultMapper.Map(withFee, References(withFee.Shopping)).Data.OtherService!.Ancillary.Pricing.Fees.Single(fee => fee.OrderLevel).Amount);
    }

    [Fact]
    public async Task V123_SH_01_seat_offers_hold_only_the_seat_family_and_ancillary_offers_hold_every_other_family()
    {
        var lab = Shop();
        var offers = await ServiceAsync(lab);
        var seats = await offers.ExecuteAsync(new OfferQuery(OfferWire.OfferId), Desk, OfferFamilyFilters.Seat);
        var services = await offers.ExecuteAsync(new OfferQuery(OfferWire.OfferId), Desk, OfferFamilyFilters.Services);
        var api = ApiSeatOffersMapper.Map(seats, References(seats));
        var panel = OtaPanelSeatOffersMapper.Map(seats);
        var backoffice = BackofficeSeatOffersMapper.Map(seats);

        Assert.Equal(4, seats.Items.Count());
        Assert.All(seats.Items, item => Assert.Equal((AncillaryProfile.Seat, "A07", SeatPurpose.Standard), (item.Candidate.Profile, item.Candidate.VariantCode, item.Candidate.Seat!.Purpose)));
        Assert.Equal(8, services.Items.Count());
        Assert.DoesNotContain(services.Items, item => item.Candidate.Profile == AncillaryProfile.Seat);
        Assert.All(services.Items, item => Assert.Null(item.Candidate.Seat));
        Assert.Equal((ApiSeatOffersMapper.SeatOffers, 4, 0), (api.Data.Type, api.Included.SeatOffers.Count, api.Meta.WithheldCount));
        Assert.All(api.Included.SeatOffers.Values, offer => Assert.Equal(("SEATS", "Standard", "1500000.00", "IRR"), (offer.Ancillary.Type, offer.Seat.Purpose, offer.Price!.Amount, offer.Price.CurrencyCode)));
        Assert.Equal(4, panel.TravellerGroups.SelectMany(group => group.FlightGroups).Sum(group => group.SeatOffers.Count));
        Assert.Equal((4, 0), (backoffice.Offers.Count, backoffice.BlockedOffers.Count));
        Assert.All(backoffice.Offers, offer => Assert.Equal(("Seat", "Standard", 1_500_000m), (offer.Service.Profile.Name, offer.Seat.Purpose.Name, offer.Pricing.CompleteUnitTotal)));
        Assert.DoesNotContain("\"availability\":", JsonSerializer.Serialize(api, Json), StringComparison.Ordinal);
        Assert.DoesNotContain("\"availability\":", JsonSerializer.Serialize(backoffice, Json), StringComparison.Ordinal);
        Assert.DoesNotContain("\"availability\":", JsonSerializer.Serialize(panel, Json), StringComparison.Ordinal);
        Assert.DoesNotContain(ApiAncillaryOffersMapper.Map(services, References(services)).Included.OtherServices.Values, service => service.Ancillary.Type == "SEATS");
    }

    [Fact]
    public async Task V123_SH_11_an_offer_item_ref_of_one_family_is_not_found_by_the_other_family_on_any_operation()
    {
        var lab = Shop();
        var offers = await ServiceAsync(lab);
        var details = new GetAncillaryOfferDetailService(offers, lab.Catalog, lab.Catalog, lab.Catalog);
        var selections = new EvaluateSelectionService(offers, lab.Engine);
        var seat = (await offers.ExecuteAsync(new OfferQuery(OfferWire.OfferId), Desk, OfferFamilyFilters.Seat)).Items.First();
        var bag = (await offers.ExecuteAsync(new OfferQuery(OfferWire.OfferId), Desk, OfferFamilyFilters.Services)).Items.First(item => item.Candidate.VariantCode == "A01");
        var seatQuery = new OfferQuery(OfferWire.OfferId, seat.OfferItemRef);
        var bagQuery = new OfferQuery(OfferWire.OfferId, bag.OfferItemRef);

        foreach (var crossed in new Func<Task>[]
                 {
                     () => offers.FindItemAsync(seatQuery, Desk, OfferFamilyFilters.Services),
                     () => offers.FindItemAsync(bagQuery, Desk, OfferFamilyFilters.Seat),
                     () => details.ExecuteAsync(seatQuery, Desk, OfferFamilyFilters.Services),
                     () => details.ExecuteAsync(bagQuery, Desk, OfferFamilyFilters.Seat),
                     () => selections.ExecuteAsync(seatQuery, new AncillarySelection { SeatNumber = "12A" }, Desk, OfferFamilyFilters.Services),
                     () => selections.ExecuteAsync(bagQuery, new AncillarySelection { Quantity = 1 }, Desk, OfferFamilyFilters.Seat)
                 })
        {
            var missing = await Assert.ThrowsAsync<BusinessException>(crossed);

            Assert.Equal((16811, 404), (missing.Code, missing.HttpStatus));
        }

        var detail = await details.ExecuteAsync(seatQuery, Desk, OfferFamilyFilters.Seat);
        var selection = await selections.ExecuteAsync(seatQuery, ApiSelectionInputMapper.ToSelection(new ApiAncillarySelectionInput { SeatNumber = "12A" }), Desk, OfferFamilyFilters.Seat);
        var api = ApiSeatOffersMapper.Detail(detail, References(detail.Shopping, detail)).Data;
        var chosen = ApiSeatOffersMapper.Selection(selection, References(selection.Shopping)).Data;

        Assert.Equal((ApiSeatOffersMapper.SeatOffer, seat.OfferItemRef, "Standard"), (api.Type, api.Id, api.Specification.SeatPurpose));
        Assert.Equal("Standard", BackofficeSeatOffersMapper.Detail(detail).Specification.SeatPurpose.Name);
        Assert.Equal("Standard", OtaPanelSeatOffersMapper.Detail(detail).Specification.SeatPurpose);
        Assert.Equal((ApiSeatOffersMapper.SeatSelection, "Incomplete", "NeedsVerification", false), (chosen.Type, chosen.Status, chosen.SeatOffer.Ancillary.Readiness, chosen.CanPurchase));
        Assert.Null(chosen.SelectedPrice);
        Assert.Contains(ShoppingReasonCodes.SeatMapNotVerified, selection.Evaluation.Candidate.ReasonCodes);
        Assert.Equal("Incomplete", BackofficeSeatOffersMapper.Selection(selection).Status.Name);
        Assert.Equal("Incomplete", OtaPanelSeatOffersMapper.Selection(selection).Status);
        Assert.Null(BackofficeSeatOffersMapper.Selection(selection).SelectedQuantityTotal);

        var verified = await selections.ExecuteAsync(seatQuery, new AncillarySelection { SeatNumber = "12A" }, Desk, OfferFamilyFilters.Seat);

        Assert.Equal(SelectionEvaluationStatus.Incomplete, verified.Evaluation.Status);
    }

    [Fact]
    public void V123_SRF_06_three_seat_controllers_expose_three_operations_on_three_seat_routes_under_the_same_three_policies()
    {
        var controllers = new[] { typeof(SeatControllers.BackofficeController), typeof(SeatControllers.OtaController), typeof(SeatControllers.OtaPanelController) }
            .Select(type => (
                Route: type.GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Single().Template,
                Policy: type.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Policy,
                Actions: type.GetMethods()
                    .SelectMany(method => method.GetCustomAttributes(typeof(HttpPostAttribute), false).Cast<HttpPostAttribute>().Select(post => (post.Template ?? string.Empty, method.GetParameters()[0].ParameterType)))
                    .OrderBy(action => action.Item1, StringComparer.Ordinal)
                    .ToList()))
            .ToList();

        Assert.Equal(
            new[]
            {
                ("Backoffice/v{version:apiVersion}/SeatOffers", SurfaceAuthorization.Backoffice),
                ("Api/v{version:apiVersion}/SeatOffers", SurfaceAuthorization.Ota),
                ("OtaPanel/v{version:apiVersion}/SeatOffers", SurfaceAuthorization.OtaPanel)
            },
            controllers.Select(controller => (controller.Route, controller.Policy!)));
        Assert.All(controllers, controller => Assert.Equal(new[] { "", "Details", "Selections/Evaluate" }, controller.Actions.Select(action => action.Item1)));
        Assert.Equal(9, controllers.SelectMany(controller => controller.Actions.Select(action => action.Item2)).Distinct().Count());
        Assert.All(controllers.SelectMany(controller => controller.Actions), action => Assert.Contains("Seat", action.Item2.Name, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("backoffice")]
    [InlineData("api")]
    [InlineData("otapanel")]
    public async Task P32B_GOLD_01_the_serialized_answers_of_the_three_surfaces_match_their_golden_files(string surface)
    {
        var lab = Shop();

        lab.Sellable("A10");

        var scope = surface == "backoffice" ? (AncillarySalesScope)Desk : Desk.On(AgencySalesPolicy.SalesChannel, Pos);
        var (offers, snapshot) = await ShoppedAsync(lab, scope);
        var bag = snapshot.Items.First(item => item.Candidate.VariantCode == "A01");
        var query = new OfferQuery(OfferWire.OfferId, bag.OfferItemRef);
        var detail = await new GetAncillaryOfferDetailService(offers, lab.Catalog, lab.Catalog, lab.Catalog).ExecuteAsync(query, scope);
        var selection = await new EvaluateSelectionService(offers, lab.Engine).ExecuteAsync(query, new AncillarySelection { Quantity = 2 }, scope);
        var answers = surface switch
        {
            "backoffice" => new (string Name, object Answer)[]
            {
                ("offers", BackofficeAncillaryOffersMapper.Map(snapshot)), ("detail", BackofficeAncillaryOfferDetailMapper.Map(detail)), ("selection", BackofficeSelectionResultMapper.Map(selection))
            },
            "api" =>
            [
                ("offers", ApiAncillaryOffersMapper.Map(snapshot, References(snapshot, detail))),
                ("detail", ApiAncillaryOfferDetailMapper.Map(detail, References(detail.Shopping, detail))),
                ("selection", ApiSelectionResultMapper.Map(selection, References(selection.Shopping, detail)))
            ],
            _ => [("offers", OtaPanelAncillaryOffersMapper.Map(snapshot)), ("detail", OtaPanelAncillaryOfferDetailMapper.Map(detail)), ("selection", OtaPanelSelectionResultMapper.Map(selection))]
        };

        foreach (var (name, answer) in answers)
        {
            var actual = JsonSerializer.Serialize(new { data = answer }, Json).ReplaceLineEndings("\n");
            var file = $"{surface}-{name}.json";

            if (Environment.GetEnvironmentVariable("P32B_WRITE_GOLDEN") == "1")
                await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Fixtures", "Golden", file), actual.ReplaceLineEndings("\r\n"));
            else
                Assert.Equal((await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Golden", file))).ReplaceLineEndings("\n"), actual);
        }
    }
}
