using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Providers.SeatMap;
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
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetSeatMap;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetSeatMap.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetSeatMap.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetSeatMap.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.RestApi.V1.AncillaryOffers.Controllers;
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

    private static IEnumerable<ApiOfferItemDto> Items(ApiAncillaryOffersDto response) => response.ALaCarteOffers.SelectMany(offer => offer.OfferItems);

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
        var api = ApiAncillaryOffersMapper.Map(snapshot);
        var panel = OtaPanelAncillaryOffersMapper.Map(snapshot);
        var offered = snapshot.Items.Where(item => ApiAncillaryOffersMapper.IsOffered(item.Candidate.OfferReadiness)).ToList();

        Assert.Equal(12, snapshot.Items.Count());
        Assert.Equal(12, offered.Count);
        Assert.Equal((12, 0), (backoffice.Offers.Count, backoffice.BlockedOffers.Count));
        Assert.Equal((12, 0), (Items(api).Count(), api.WithheldItemCount));
        Assert.Equal((12, 0), (Services(panel).Count(), panel.BlockedCount));
        Assert.All(offered, item =>
        {
            var price = item.Candidate.Price;
            var operatorView = backoffice.Offers.Single(offer => offer.OfferItemRef == item.OfferItemRef);
            var agencyView = Items(api).Single(offer => offer.OfferItemRef == item.OfferItemRef);
            var panelView = Services(panel).Single(service => service.OfferItemRef == item.OfferItemRef);

            Assert.Equal((price.CompleteUnitTotal, price.CurrencyId), (operatorView.Pricing.CompleteUnitTotal, operatorView.Pricing.CurrencyId));
            Assert.Equal((price.CompleteUnitTotal, price.CurrencyId), (agencyView.Pricing.UnitPrice?.Amount, agencyView.Pricing.UnitPrice?.CurrencyId));
            Assert.Equal((price.CompleteUnitTotal, price.CurrencyId), (panelView.UnitPrice?.Amount, panelView.UnitPrice?.CurrencyId));
            Assert.Equal(item.Candidate.OfferReadiness.ToString(), operatorView.OfferReadiness.Name);
            Assert.Equal(item.Candidate.OfferReadiness.ToString(), agencyView.Readiness);
            Assert.Equal(OtaPanelAncillaryOffersMapper.DisplayStatusOf(item.Candidate.OfferReadiness), panelView.DisplayStatus);
            Assert.Equal(price.Status.ToString(), operatorView.Pricing.Status.Name);
            Assert.Equal(price.Status.ToString(), agencyView.Pricing.Status);
            Assert.Equal(price.Status.ToString(), panelView.PriceStatus);
            Assert.Equal(item.Candidate.FlightRefs, operatorView.Coverage.FlightRefs);
            Assert.Equal(item.Candidate.FlightRefs, agencyView.Eligibility.SegmentRefs);
            Assert.Equal(item.Candidate.FlightRefs, panelView.CoveredFlightRefs);
            Assert.Equal(price.UnappliedFeeLines.Select(line => line.Amount), agencyView.Pricing.OrderLevelFees.Select(line => line.Amount));
            Assert.Equal(price.UnappliedFeeLines.Select(line => line.Amount), panelView.OrderLevelFees.Select(line => line.Amount));
            Assert.False(operatorView.CanPurchase || agencyView.CanPurchase || panelView.CanPurchase);
            Assert.Null(agencyView.Pricing.TotalPrice);
        });
        Assert.Equal(3, new[] { backoffice.GetType(), api.GetType(), panel.GetType() }.Distinct().Count());
        Assert.Empty(Members(typeof(ApiAncillaryOffersDto)).Intersect(Members(typeof(BackofficeAncillaryOffersDto))).Except(["Currency"]));
        Assert.Equal(new[] { "BlockedCount", "CurrencyCode", "CurrencyId", "OfferId", "TravellerGroups" }, Members(typeof(OtaPanelAncillaryOffersDto)));
    }

    [Fact]
    public async Task P32B_SRF_02_the_agency_api_is_normalised_and_shows_no_internal_identifier_or_diagnostic()
    {
        var lab = Shop();
        var blocked = lab.Definition("A13");

        lab.Price(blocked, lab.Provision(blocked), Rate(2_000_000m, Irr));

        var snapshot = await SnapshotAsync(lab, scope: Desk.On(AgencySalesPolicy.SalesChannel, Pos));
        var api = ApiAncillaryOffersMapper.Map(snapshot);
        var json = JsonSerializer.Serialize(api, Json);
        var withheld = snapshot.Items.Where(item => !ApiAncillaryOffersMapper.IsOffered(item.Candidate.OfferReadiness)).ToList();

        Assert.NotEmpty(withheld);
        Assert.Equal(withheld.Count, api.WithheldItemCount);
        Assert.Equal(new[] { "ADT-1", "CHD-1" }, api.DataLists.Passengers.Select(passenger => passenger.PassengerRef));
        Assert.Equal(new[] { "ADT", "CHD" }, api.DataLists.Passengers.Select(passenger => passenger.Ptc));
        Assert.Equal(new[] { "B1:F1", "B2:F1" }, api.DataLists.PaxSegments.Select(segment => segment.SegmentRef));
        Assert.Equal(3, api.DataLists.ServiceDefinitions.Count);
        Assert.DoesNotContain(api.DataLists.ServiceDefinitions, definition => definition.VariantCode == "A13");
        Assert.Equal(api.DataLists.ServiceDefinitions.Count, api.DataLists.ServiceDefinitions.Select(definition => definition.ServiceDefinitionRef).Distinct().Count());
        Assert.Single(api.ALaCarteOffers);
        Assert.All(Items(api), item =>
        {
            Assert.Contains(api.DataLists.ServiceDefinitions, definition => definition.ServiceDefinitionRef == item.ServiceDefinitionRef);
            Assert.All(item.Eligibility.PassengerRefs, reference => Assert.Contains(api.DataLists.Passengers, passenger => passenger.PassengerRef == reference));
            Assert.All(item.Eligibility.SegmentRefs, reference => Assert.Contains(api.DataLists.PaxSegments, segment => segment.SegmentRef == reference));
            Assert.False(item.Availability.Guaranteed);
            Assert.NotEqual("NeedsVerification", item.Readiness);
        });
        Assert.Equal(ApiAncillaryOffersMapper.NeedsSelectionOrRecheck, api.ResponseStatus);
        Assert.All(
            new[] { "provisionId", "pricingRevisionId", "pricingRateId", "definitionVersionId", "pointOfSale", "supplierId", "diagnostics", "missingContextFields", "ownerAirlineId" },
            member => Assert.DoesNotContain(member, json, StringComparison.OrdinalIgnoreCase));
        Assert.All(
            snapshot.Items.SelectMany(item => new[] { item.Candidate.ProvisionId, item.Candidate.DefinitionVersionId, item.Candidate.Price.PricingRevisionId ?? 0, item.PointOfSaleId }).Where(id => id > 999).Distinct(),
            id => Assert.DoesNotContain($"\"{id}\"", json, StringComparison.Ordinal));
        Assert.Contains("\"unitPrice\": {", json, StringComparison.Ordinal);
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
        var api = ApiAncillaryOffersMapper.Map(both);

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
        Assert.Equal(new[] { 4, 4 }, api.ALaCarteOffers.Select(offer => offer.OfferItems.Count));
        Assert.Equal(new[] { 900_000m, 700_000m }, api.ALaCarteOffers.Select(offer => offer.OfferItems.Select(item => item.Pricing.UnitPrice!.Amount).Distinct().Single()));
        Assert.Single(api.DataLists.ServiceDefinitions);
        Assert.Equal((OtherPos, 4, 700_000m), (one.PointsOfSale.Single().PointOfSaleId, one.Items.Count(), one.Items.Select(item => item.Candidate.Price.CompleteUnitTotal).Distinct().Single()));
        Assert.Empty(none.PointsOfSale);
        Assert.Equal(BackofficeAncillaryOffersMapper.NoAuthorizedPointOfSale, Assert.Single(BackofficeAncillaryOffersMapper.Map(none).Diagnostics).Code);
        Assert.Equal((ApiAncillaryOffersMapper.NoApplicableOffer, 0), (ApiAncillaryOffersMapper.Map(none).ResponseStatus, ApiAncillaryOffersMapper.Map(none).ALaCarteOffers.Count));
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
        Assert.Equal(new[] { "OfferId", "OfferItemRef" }, Members(typeof(ApiGetSeatMapQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef" }, Members(typeof(OtaPanelGetSeatMapQuery)));
        Assert.Equal(new[] { "OfferId", "OfferItemRef", "Options" }, Members(typeof(BackofficeGetSeatMapQuery)));
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
    public void P32B_SRF_05_three_controllers_expose_the_same_four_operations_on_three_routes_under_three_policies()
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
        Assert.All(controllers, controller => Assert.Equal(new[] { "", "Details", "SeatMaps", "Selections/Evaluate" }, controller.Actions.Select(action => action.Item1)));
        Assert.Equal(12, controllers.SelectMany(controller => controller.Actions.Select(action => action.Item2)).Distinct().Count());
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
        var api = ApiAncillaryOfferDetailMapper.Map(detail);
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
            Assert.Equal((item.Candidate.CandidateIdentity.TravellerRef, 25m, "Kg", true), (allowance.TravellerRef, allowance.CheckedWeight, allowance.CheckedWeightUnit, allowance.Verified));
            Assert.Equal(1, allowance.CheckedPieces);
            Assert.Contains(allowance.FlightRef, item.Candidate.FlightRefs);
        });
        Assert.Equal(api.IncludedAllowance.Count, panel.IncludedAllowance.Count);
        Assert.Equal(api.IncludedAllowance.Count, backoffice.IncludedAllowance.Count);
        Assert.Equal(
            (item.Candidate.Price.CompleteUnitTotal, 5_000_000m, 450_000m),
            (api.OfferItem.Pricing.UnitPrice!.Amount, api.OfferItem.Pricing.Base!.Amount, api.OfferItem.Pricing.Taxes.Single().Amount));
        Assert.Equal((5_450_000m, 5_000_000m, 450_000m), (panel.PriceBreakdown.UnitPrice!.Amount, panel.PriceBreakdown.Base!.Amount, panel.PriceBreakdown.AddedTaxes.Single().Amount));
        Assert.Equal((5_450_000m, 5_000_000m), (backoffice.Offer.Pricing.CompleteUnitTotal, backoffice.Offer.Pricing.BaseAmount));
        Assert.Equal((5_000_000m, 5_450_000m, true, true), (api.Rates.Single().BaseAmount, api.Rates.Single().UnitTotal, api.Rates.Single().UnitTotalComplete, api.Rates.Single().Applied));
        Assert.Equal(
            (item.Candidate.Quantity.Unit.ToString(), item.Candidate.Quantity.MinPerSelection, item.Candidate.Quantity.MaxPerSelection),
            (api.OfferItem.Quantity.Unit, api.OfferItem.Quantity.Min, api.OfferItem.Quantity.Max));
        Assert.Equal(item.Candidate.Selection.Fields.Select(field => field.Name), api.OfferItem.SelectionRequirements.Select(field => field.Name));
        Assert.Equal(("Paid", "NonRefundable", definition.Booking.ConfirmationRequirement.ToString()), (api.Terms.Disposition, api.Terms.ReissueRefund, api.Terms.ConfirmationRequirement));
        Assert.Equal(("NonRefundable", false, false), (backoffice.Terms.ReissueRefund.Name, backoffice.Terms.Commissionable, backoffice.Terms.InterlineSettlement));
        Assert.Equal("Both", api.Restrictions.PurchaseStage);
        Assert.Equal(item.Candidate.OfferReadiness.ToString(), api.OfferItem.Readiness);
        Assert.False(api.OfferItem.CanPurchase || panel.Service.CanPurchase || backoffice.Offer.CanPurchase);
        Assert.False(api.OfferItem.Availability.Guaranteed);
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
        Assert.Equal(ApiAncillaryOffersMapper.NoApplicableOffer, ApiAncillaryOffersMapper.Map(snapshot).ResponseStatus);
    }

    [Theory]
    [InlineData("A07")]
    [InlineData("A08")]
    [InlineData("A09")]
    public async Task P32B_DET_02_a_seat_detail_states_the_published_seat_rules_and_the_seat_map_source_is_reported_unavailable(string code)
    {
        var lab = IrrLab();
        var definition = lab.Definition(code);

        lab.Price(definition, lab.Provision(definition), Rate(1_500_000m, Irr));

        var (offers, snapshot) = await ShoppedAsync(lab, Desk);
        var item = snapshot.Items.First();
        var query = new OfferQuery(OfferWire.OfferId, item.OfferItemRef);
        var detail = await new GetAncillaryOfferDetailService(offers, lab.Catalog, lab.Catalog, lab.Catalog).ExecuteAsync(query, Desk);
        var seat = definition.SpecificationArgs().Seat!;
        var api = ApiAncillaryOfferDetailMapper.Map(detail);
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
        Assert.All(new[] { "\"rows\"", "\"seats\"", "\"seatMap\"", "\"columns\"" }, member => Assert.DoesNotContain(member, JsonSerializer.Serialize(api, Json), StringComparison.OrdinalIgnoreCase));
        Assert.False(api.OfferItem.Availability.Guaranteed);
        Assert.Equal("NotChecked", api.OfferItem.Availability.State);

        var seatMaps = new GetSeatMapService(offers, new NotConnectedFlightSeatMapReference());
        var unavailable = await Assert.ThrowsAsync<BusinessException>(() => seatMaps.ExecuteAsync(query, Desk));

        Assert.Equal((16812, 503), (unavailable.Code, unavailable.HttpStatus));
        Assert.StartsWith("SEAT_MAP_SOURCE_UNAVAILABLE", unavailable.Message, StringComparison.Ordinal);
        Assert.Contains("SourceUnavailable", unavailable.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task P32B_DET_03_other_profiles_show_only_their_own_published_section_and_a_seat_map_is_refused_for_them()
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
            var api = ApiAncillaryOfferDetailMapper.Map(detail).Specification;
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

        var wifi = snapshot.Items.First(item => item.Candidate.VariantCode == "A24");
        var refused = await Assert.ThrowsAsync<BusinessException>(
            () => new GetSeatMapService(offers, new NotConnectedFlightSeatMapReference()).ExecuteAsync(new OfferQuery(OfferWire.OfferId, wifi.OfferItemRef), Desk));

        Assert.Equal((16801, 422), (refused.Code, refused.HttpStatus));
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
        var api = ApiSelectionResultMapper.Map(two);
        var panel = OtaPanelSelectionResultMapper.Map(two);
        var backoffice = BackofficeSelectionResultMapper.Map(two);

        Assert.Equal((SelectionEvaluationStatus.Accepted, 10_900_000m, false), (two.Evaluation.Status, two.Evaluation.Candidate.Price.RequestedQuantityTotal, two.Evaluation.ProvisionChangedBySelection));
        Assert.Equal(("Accepted", 10_900_000m, Irr, 5_450_000m), (api.Status, api.SelectedPrice!.Amount, api.SelectedPrice.CurrencyId, api.OfferItem.Pricing.UnitPrice!.Amount));
        Assert.Equal(("Accepted", 10_900_000m, 5_450_000m), (panel.Status, panel.SelectedAmount!.Amount, panel.Service.UnitPrice!.Amount));
        Assert.Equal(("Accepted", 10_900_000m, 5_450_000m), (backoffice.Status.Name, backoffice.SelectedQuantityTotal, backoffice.Offer.Pricing.CompleteUnitTotal));
        Assert.Equal(bag.OfferItemRef, api.OfferItemRef);
        Assert.Equal(bag.OfferItemRef, api.CurrentOfferItemRef);
        Assert.False(api.CanPurchase || panel.CanPurchase || backoffice.CanPurchase);
        Assert.False(api.OfferItem.Availability.Guaranteed);
        Assert.Null(bag.Candidate.Price.RequestedQuantityTotal);
        Assert.Equal((SelectionEvaluationStatus.Rejected, null), (nine.Evaluation.Status, nine.Evaluation.Candidate.Price.RequestedQuantityTotal));
        Assert.Null(OtaPanelSelectionResultMapper.Map(nine).SelectedAmount);
        Assert.Equal(("Quantity", ShoppingReasonCodes.QuantityAboveMaximum), (OtaPanelSelectionResultMapper.Map(nine).InputIssues.Single().Field, OtaPanelSelectionResultMapper.Map(nine).InputIssues.Single().Reason));
        Assert.Equal((SelectionEvaluationStatus.Incomplete, null), (none.Evaluation.Status, none.Evaluation.Candidate.Price.RequestedQuantityTotal));
        Assert.Null(BackofficeSelectionResultMapper.Map(none).SelectedQuantityTotal);
        Assert.Equal(SelectionEvaluationStatus.Rejected, forged.Evaluation.Status);
        Assert.Contains(ApiSelectionResultMapper.Map(forged).Issues, issue => issue is { Field: "SeatNumber", Reason: ShoppingReasonCodes.SelectionFieldNotAllowed });
        Assert.Equal((981_000m, null), (withFee.Evaluation.Candidate.Price.CompleteUnitTotal, withFee.Evaluation.Candidate.Price.RequestedQuantityTotal));
        Assert.Null(ApiSelectionResultMapper.Map(withFee).SelectedPrice);
        Assert.Equal(100_000m, ApiSelectionResultMapper.Map(withFee).OfferItem.Pricing.OrderLevelFees.Single().Amount);
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
            "api" => [("offers", ApiAncillaryOffersMapper.Map(snapshot)), ("detail", ApiAncillaryOfferDetailMapper.Map(detail)), ("selection", ApiSelectionResultMapper.Map(selection))],
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
