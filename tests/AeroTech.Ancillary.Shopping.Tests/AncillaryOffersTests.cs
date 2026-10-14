using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class AncillaryOffersTests
{
    [Fact]
    public async Task CT_05_CT_06_CT_08_the_context_keeps_every_traveller_coupon_and_bound_join_and_invents_no_fact()
    {
        var departure = new DateTimeOffset(2026, 12, 1, 10, 0, 0, Trip.TehranOffset);

        FlightOfferFlight Sold(int sequence, long flightId, int origin, int destination, DateTimeOffset departs, params FlightOfferLeg[] legs)
            => new(sequence, flightId, 1, "111", origin, null, destination, null, 1, 1, departs, departs.AddHours(2), 1, 1, 11, "Y", legs);

        FlightOfferFareComponent Fare(long airFareId, string boundId, string basis)
            => new(airFareId, boundId, 1, 11, "Y", basis, "LIGHT", AirFareType.Public);

        FlightOfferCoupon Coupon(string boundId, long flightId, bool baggage, params long[] airFareIds)
            => new(boundId, flightId, baggage ? new FlightOfferBaggage(1, 23m, WeightUnit.Kg) : null, baggage ? new FlightOfferBaggage(1, 8m, WeightUnit.Kg) : null, airFareIds);

        var offer = new FlightOffer(
            "OFFER",
            Now,
            Now.AddDays(1),
            null,
            Eur,
            "EUR",
            300m,
            30m,
            330m,
            [
                new FlightOfferBound(
                    "B1",
                    1,
                    BoundDirection.Outbound,
                    Trip.Thr,
                    Trip.Ist,
                    [
                        Sold(
                            1,
                            501,
                            Trip.Thr,
                            Trip.Ika,
                            departure,
                            new FlightOfferLeg(1, 1, Trip.Thr, Trip.Mhd, departure, departure.AddMinutes(50)),
                            new FlightOfferLeg(2, 2, Trip.Mhd, Trip.Ika, departure.AddMinutes(70), departure.AddHours(2))),
                        Sold(2, 502, Trip.Ika, Trip.Ist, departure.AddHours(4))
                    ]),
                new FlightOfferBound("B2", 2, BoundDirection.Inbound, Trip.Ist, Trip.Thr, [Sold(1, 503, Trip.Ist, Trip.Thr, departure.AddDays(7))])
            ],
            [Fare(9001, "B1", "YADT"), Fare(9002, "B1", "YCHD"), Fare(9003, "B2", "YRET")],
            [
                new FlightOfferTicket("ADT-1", 1, PassengerTypeCode.ADT, [Coupon("B1", 501, true, 9001), Coupon("B1", 502, true, 9001), Coupon("B2", 503, true, 9003)]),
                new FlightOfferTicket("CHD-1", 2, PassengerTypeCode.CHD, [Coupon("B1", 501, true, 9002), Coupon("B1", 502, true, 9002), Coupon("B2", 503, true, 9003)]),
                new FlightOfferTicket("INF-1", 3, PassengerTypeCode.INF, [Coupon("B1", 501, false, 9001, 9002), Coupon("B1", 502, false, 9003), Coupon("B2", 503, false)])
            ]);
        var context = ContextOf(offer, Sale with { CurrencyId = null }, new Dictionary<int, int> { [Trip.Thr] = 100, [Trip.Ist] = 200 });

        Assert.Equal(new[] { ("B1", "B1:F1,B1:F2", Trip.Ika), ("B2", "B2:F1", 0) }, context.Portions.Select(portion => (portion.PortionRef, string.Join(',', portion.FlightRefs), portion.ViaAirportIds!.SingleOrDefault())));
        Assert.Equal(new[] { 501L, 502L, 503L }, context.Flights.Select(flight => flight.FlightId));
        Assert.Equal(new[] { Trip.Mhd }, context.Flights[0].ViaAirportIds);
        Assert.Equal(((int?)100, (int?)null, (int?)200), (context.Flights[0].OriginCountryId, context.Flights[0].DestinationCountryId, context.Flights[1].DestinationCountryId));
        Assert.All(context.Flights, flight => Assert.Null(flight.OriginIanaTimeZoneId));
        Assert.Equal(new[] { PassengerTypeCode.ADT, PassengerTypeCode.CHD, PassengerTypeCode.INF }, context.Travellers.Select(traveller => traveller.PassengerTypeCode));
        Assert.All(context.Travellers, traveller => Assert.Equal((null, null, null, null), (traveller.DateOfBirth, traveller.VerifiedAgeAtTravel, traveller.StableTravellerIdentity, traveller.VerifiedExitRowEligible)));
        Assert.Equal(
            new[] { ("ADT-1", "B1:F1", 9001L, "YADT"), ("ADT-1", "B1:F2", 9001L, "YADT"), ("ADT-1", "B2:F1", 9003L, "YRET"), ("CHD-1", "B1:F1", 9002L, "YCHD"), ("CHD-1", "B1:F2", 9002L, "YCHD"), ("CHD-1", "B2:F1", 9003L, "YRET") },
            context.TravellerFareFacts.Select(fact => (fact.TravellerRef, fact.FlightRef, fact.AirFareId!.Value, fact.FareBasisCode!)));
        Assert.All(context.TravellerFareFacts, fact => Assert.Equal(((long?)null, "LIGHT"), (fact.FareFamilyId, fact.FareFamilyCodeOrName)));
        Assert.Equal(6, context.TravellerBaggageFacts.Count);
        Assert.DoesNotContain(context.TravellerBaggageFacts, fact => fact.TravellerRef == "INF-1");
        Assert.Equal(
            (true, true, false, false, false, false),
            (context.CoverageCompleteness.TravellersComplete, context.CoverageCompleteness.ItineraryComplete, context.CoverageCompleteness.FareFactsComplete, context.CoverageCompleteness.BaggageFactsComplete, context.CoverageCompleteness.FareBenefitsComplete, context.CoverageCompleteness.ExistingServicesComplete));
        Assert.Equal((ShoppingSourceKind.Offer, "OFFER", null, null), (context.SourceIdentity.SourceKind, context.SourceIdentity.TrustedSourceReference, context.SourceIdentity.ValidUntilUtc, context.SourceIdentity.OrderId));
        Assert.Equal((ShoppingStage.PreOrder, Eur, Now, Airline, Pos), (context.ShoppingStage, context.CurrencyId, context.EvaluatedAtUtc, context.OwnerAirlineId, context.PointOfSaleId));

        var lab = new ShoppingLab();

        lab.Sellable("A01");
        lab.Sellable("A24");

        var result = await lab.Engine.ShopAsync(context);
        var bags = result.Candidates.Where(candidate => candidate.VariantCode == "A01").ToList();
        var wifi = result.Candidates.Where(candidate => candidate.VariantCode == "A24").ToList();

        Assert.Equal(6, bags.Count);
        Assert.Equal(new[] { "B1:F1", "B1:F2" }, bags.Single(candidate => candidate.CandidateIdentity is { TravellerRef: "ADT-1", CoverageRef: "B1" }).FlightRefs);
        Assert.All(bags.Where(candidate => candidate.CandidateIdentity.TravellerRef != "INF-1"), candidate => Assert.Equal(OfferReadiness.Selectable, candidate.OfferReadiness));
        Assert.All(bags.Where(candidate => candidate.CandidateIdentity.TravellerRef == "INF-1"), candidate => Assert.Equal(OfferReadiness.NeedsVerification, candidate.OfferReadiness));
        Assert.Equal(9, wifi.Count);
        Assert.All(wifi, candidate => Assert.Single(candidate.FlightRefs));
    }

    [Fact]
    public async Task CT_19_a_valid_offer_without_any_published_product_is_a_journey_with_an_explicit_no_product_diagnostic()
    {
        var response = await ShopAsync(new ShoppingLab());

        Assert.Equal((OfferWire.OfferId, Now, Eur), (response.OfferId, response.EvaluatedAtUtc, response.Currency.CurrencyId));
        Assert.Null(response.Currency.CurrencyCode);
        Assert.Equal((Eur, 480000000m, 528010024m), (response.FlightOffer.CurrencyId, response.FlightOffer.BaseAmount, response.FlightOffer.TotalAmount));
        Assert.Null(response.FlightOffer.LastTicketingDate);
        Assert.Equal((Airline, Pos, "PreOrder"), (response.Scope.OwnerAirlineId, response.Scope.PointOfSaleId!.Value, response.Scope.ShoppingStage.Name));
        Assert.Equal(new[] { ("B1", "Outbound", "B1:F1"), ("B2", "Inbound", "B2:F1") }, response.Journey.Portions.Select(portion => (portion.PortionRef, portion.Direction!.Name, portion.Flights.Single().FlightRef)));
        Assert.Equal(new[] { ("ADT-1", "ADT"), ("CHD-1", "CHD") }, response.Travellers.Select(traveller => (traveller.TravellerRef, traveller.PassengerTypeCode.Name)));
        Assert.All(response.Travellers, traveller => Assert.Equal(new[] { "B1:F1", "B2:F1" }, traveller.FareByFlight.Select(fare => fare.FlightRef)));
        Assert.Empty(response.Offers);
        Assert.Empty(response.BlockedOffers);
        Assert.Empty(response.Categories);
        Assert.Equal(BackofficeAncillaryOffersMapper.NoApplicableProduct, Assert.Single(response.Diagnostics).Code);
        Assert.Equal((true, false, false, false), (response.Completeness.FlightOfferVerified, response.Completeness.ShoppingContextComplete, response.Completeness.HasBlockedProducts, response.Completeness.LiveSupplierAvailabilityChecked));
    }

    [Fact]
    public async Task CT_11_CT_23_each_candidate_keeps_its_own_unit_price_lines_and_the_flight_amounts_stay_separate()
    {
        var lab = new ShoppingLab();
        var wifi = lab.Definition("A24");

        lab.Price(wifi, lab.Provision(wifi), Rate(100m, components: [Tax("VAT", 9m, TaxTreatment.AddedToBase), Tax("INC", 5m, TaxTreatment.IncludedInBase), Fee("TKT", 7m, FeeApplicationUnit.Ticket)]));

        var response = await ShopAsync(lab);

        Assert.Equal(4, response.Offers.Count);
        Assert.Equal(("Connectivity", 4), (Assert.Single(response.Categories).Code.Name, response.Categories[0].OfferCount));
        Assert.Equal(
            new[] { ("ADT-1", "B1:F1"), ("ADT-1", "B2:F1"), ("CHD-1", "B1:F1"), ("CHD-1", "B2:F1") },
            response.Offers.Select(offer => (offer.Coverage.TravellerRefs.Single(), offer.Coverage.FlightRefs.Single())).Order());
        Assert.All(response.Offers, offer =>
        {
            Assert.Equal(("Filed", "Complete", Eur, "PerItem", 100m, 109m, false), (offer.Pricing.Origin.Name, offer.Pricing.Status.Name, offer.Pricing.CurrencyId, offer.Pricing.PricingUnit!.Name, offer.Pricing.BaseAmount, offer.Pricing.CompleteUnitTotal, offer.Pricing.IsOrderLevelTotalComplete));
            Assert.Equal(("VAT", 9m), (offer.Pricing.AddedTaxes.Single().Code, offer.Pricing.AddedTaxes.Single().Amount));
            Assert.Equal(("INC", 5m), (offer.Pricing.IncludedTaxes.Single().Code, offer.Pricing.IncludedTaxes.Single().Amount));
            Assert.Equal(("TKT", 7m, "Ticket"), (offer.Pricing.UnappliedFees.Single().Code, offer.Pricing.UnappliedFees.Single().Amount, offer.Pricing.UnappliedFees.Single().FeeApplicationUnit!.Name));
            Assert.Equal(("NeedsSelection", "NeedsInput", false, false), (offer.OfferReadiness.Name, offer.DisplayState.Name, offer.CanPurchase, offer.Availability.IsGuaranteed));
            Assert.Equal(("A24", "Sector"), (offer.Service.VariantCode, offer.CandidateIdentity.CoverageScope.Name));
        });
        Assert.Equal(528010024m, response.FlightOffer.TotalAmount);
    }

    [Fact]
    public async Task CT_09_CT_12_CT_13_CT_14_missing_evidence_blocks_only_the_products_that_need_it()
    {
        var lab = new ShoppingLab();
        var dated = lab.Definition("A24", "WIFI_DATED");

        lab.Sellable("A24");
        lab.Sellable("A07");
        lab.Sellable("A10");
        lab.Sellable("A15");
        lab.Price(dated, lab.Provision(dated, rules: Rules(dayTime: new([new(127, new TimeOnly(0, 0), new TimeOnly(23, 59), DayTimeRestrictionEffect.Allow)]))), Rate(40m));

        var response = await ShopAsync(lab);

        IReadOnlyList<BackofficeAncillaryOfferDto> Of(IReadOnlyList<BackofficeAncillaryOfferDto> offers, string reference) => offers.Where(offer => offer.Service.ServiceDefinitionRef == reference).ToList();

        var seat = Of(response.Offers, lab.Catalog.Definitions.Single(definition => definition.VariantCode == "A07").ServiceDefinitionRef);
        var upgrade = Of(response.BlockedOffers, lab.Catalog.Definitions.Single(definition => definition.VariantCode == "A10").ServiceDefinitionRef);
        var wheelchair = Of(response.BlockedOffers, lab.Catalog.Definitions.Single(definition => definition.VariantCode == "A15").ServiceDefinitionRef);

        Assert.Equal(8, response.Offers.Count);
        Assert.Equal(new[] { "Seat", "Connectivity" }, response.Categories.Select(category => category.Code.Name));
        Assert.All(seat, offer =>
        {
            Assert.Equal(("SeatMapSelection", "NeedsInput", false, false), (offer.Selection.Kind.Name, offer.DisplayState.Name, offer.Availability.IsGuaranteed, offer.CanPurchase));
            Assert.Contains(offer.Selection.Fields, field => field is { Name: "SeatNumber", Required: true });
            Assert.Equal("NotRequested", offer.Confirmation.ProviderConfirmationStatus.Name);
        });
        Assert.All(upgrade, offer =>
        {
            Assert.Equal(("ExternalQuote", "QuoteRequired", "NeedsVerification"), (offer.Pricing.Origin.Name, offer.Pricing.Status.Name, offer.DisplayState.Name));
            Assert.Null(offer.Pricing.BaseAmount);
            Assert.Null(offer.Pricing.CompleteUnitTotal);
            Assert.Contains(ShoppingReasonCodes.FareFamilyNotVerified, offer.ReasonCodes);
        });
        Assert.All(wheelchair, offer =>
        {
            Assert.Equal(("Free", "NeedsVerification"), (offer.Pricing.Origin.Name, offer.DisplayState.Name));
            Assert.Null(offer.Pricing.CompleteUnitTotal);
            Assert.Empty(offer.Pricing.AddedTaxes.Concat(offer.Pricing.AppliedUnitFees).Concat(offer.Pricing.UnappliedFees));
            Assert.Contains(ShoppingReasonCodes.TimeZoneNotVerified, offer.ReasonCodes);
        });
        Assert.All(Of(response.BlockedOffers, "WIFI_DATED"), offer => Assert.Contains(ShoppingReasonCodes.TimeZoneNotVerified, offer.Eligibility.ReasonCodes));
        Assert.Equal(4, Of(response.BlockedOffers, "WIFI_DATED").Count);
        Assert.True(response.Completeness.HasBlockedProducts);
        Assert.Contains(response.Diagnostics, diagnostic => diagnostic is { Code: ShoppingReasonCodes.TimeZoneNotVerified, Severity.Name: "Blocker" });
        Assert.All(response.Travellers.SelectMany(traveller => traveller.FareByFlight), fare => Assert.Equal(("Flex", (long?)null), (fare.FareFamilyText, fare.FareFamilyId)));
    }

    [Fact]
    public async Task CT_09_the_fare_family_id_is_read_from_the_air_fare_reference_and_never_from_the_fare_family_text()
    {
        var lab = new ShoppingLab();
        var airFares = new TestOnlyAirFareReference();

        lab.Sellable("A10");

        foreach (var component in (await OfferWire.ReadAsync(OfferWire.Captured())).FareComponents)
            airFares.FareFamilies[component.AirFareId] = 77;

        var unverified = await ShopAsync(lab);
        var verified = await ShopAsync(lab, airFares: airFares);

        Assert.All(unverified.Travellers.SelectMany(traveller => traveller.FareByFlight), fare => Assert.Null(fare.FareFamilyId));
        Assert.All(unverified.BlockedOffers, offer => Assert.Contains(ShoppingReasonCodes.FareFamilyNotVerified, offer.ReasonCodes));
        Assert.NotEmpty(unverified.BlockedOffers);
        Assert.All(verified.Travellers.SelectMany(traveller => traveller.FareByFlight), fare => Assert.Equal(("Flex", (long?)77), (fare.FareFamilyText, fare.FareFamilyId)));
        Assert.All(verified.Offers.Concat(verified.BlockedOffers), offer => Assert.DoesNotContain(ShoppingReasonCodes.FareFamilyNotVerified, offer.ReasonCodes));
    }

    [Fact]
    public async Task CT_10_CT_15_CT_16_an_unknown_allowance_scale_or_usage_is_shown_as_blocked_not_as_sellable()
    {
        JsonObject Coupon(JsonObject data, int ticket, int coupon) => data["tickets"]![ticket]!["coupons"]![coupon]!.AsObject();

        var bags = new ShoppingLab();

        bags.Sellable("A01");

        var known = await ShopAsync(bags);
        var unknown = await ShopAsync(bags, OfferWire.Changed(data => Coupon(data, 0, 0)["baggageUnit"] = ""));
        var zero = await ShopAsync(bags, OfferWire.Changed(data => Coupon(data, 0, 0)["baggagePieces"] = 0));

        Assert.Equal((4, 0), (known.Offers.Count, known.BlockedOffers.Count));
        Assert.All(known.Offers, offer => Assert.Equal(("Selectable", "AvailableForSelection", "Portion", false), (offer.OfferReadiness.Name, offer.DisplayState.Name, offer.Coverage.Scope.Name, offer.CanPurchase)));
        Assert.Equal((3, 1), (unknown.Offers.Count, unknown.BlockedOffers.Count));
        Assert.Equal(("ADT-1", "B1"), (unknown.BlockedOffers[0].CandidateIdentity.TravellerRef, unknown.BlockedOffers[0].CandidateIdentity.CoverageRef));
        Assert.Contains(ShoppingReasonCodes.BaggageAllowanceNotVerified, unknown.BlockedOffers[0].ReasonCodes);
        Assert.Null(unknown.Travellers[0].IncludedBaggageByFlight[0].CheckedPieces);
        Assert.Equal(1, unknown.Travellers[0].IncludedBaggageByFlight[0].CabinPieces);
        Assert.Equal((4, 0, 0), (zero.Offers.Count, zero.BlockedOffers.Count, zero.Travellers[0].IncludedBaggageByFlight[0].CheckedPieces));

        var noScale = new ShoppingLab();

        noScale.Sellable("A01");
        noScale.Catalog.CurrencyScales.Remove(Eur);

        var unpriced = await ShopAsync(noScale);

        Assert.Empty(unpriced.Offers);
        Assert.All(unpriced.BlockedOffers, offer =>
        {
            Assert.Equal(("Incomplete", "NeedsVerification"), (offer.Pricing.Status.Name, offer.DisplayState.Name));
            Assert.Null(offer.Pricing.CompleteUnitTotal);
        });

        var limited = new ShoppingLab { Catalog = { FamilyAnswer = InventoryReferenceCheck.Verified } };
        var (bag, _) = limited.Sellable("A01");

        limited.Policy(bag, InventoryAuthority.Unlimited, limits: new PassengerUsageLimitArgs(PassengerUsageLimitScope.PerPortion, 3, "XBAG", UsageConsumptionUnit.PurchasedUnit, null));

        var metered = await ShopAsync(limited);

        Assert.Empty(metered.Offers);
        Assert.All(metered.BlockedOffers, offer =>
        {
            Assert.Contains(ShoppingReasonCodes.UsageNotVerified, offer.ReasonCodes);
            Assert.Null(Assert.Single(offer.Quantity.CumulativeLimits).VerifiedRemaining);
        });
    }

    [Fact]
    public async Task CT_20_every_variant_is_projected_with_its_selection_contract_and_one_display_state()
    {
        var lab = new ShoppingLab();

        foreach (var variant in V122Catalog.Cases)
            lab.Sellable(variant.Code);

        var outbound = Trip.Flight("F1", 101, Trip.Thr, Trip.Ist);
        var inbound = Trip.Flight("F2", 102, Trip.Ist, Trip.Ika, outbound.DepartureAt.AddDays(7), Trip.Istanbul);
        var trip = Trip.Context([outbound, inbound], [Trip.Portion("P1", 1, outbound), Trip.Portion("P2", 2, inbound)], [Trip.Adult("T1"), Trip.Child("T2")]);
        var result = await lab.Engine.ShopAsync(trip with
        {
            TravellerBaggageFacts = trip.TravellerBaggageFacts.Select(fact => fact.FlightRef == "F2" ? Trip.WeightAllowance(fact.TravellerRef, "F2", "P2") : fact).ToList()
        });
        var projected = result.Candidates.Select(candidate => (Candidate: candidate, Offer: BackofficeAncillaryOfferMapper.Map("item", Pos, candidate))).ToList();

        Assert.Equal(V122Catalog.Cases.Select(variant => variant.Code).Order(), projected.Select(row => row.Offer.Service.VariantCode).Distinct().Order());
        Assert.All(projected, row =>
        {
            var expected = V122Catalog.Selection[row.Offer.Service.VariantCode];

            Assert.Equal(expected.Kind.ToString(), row.Offer.Selection.Kind.Name);
            Assert.Equal(expected.Fields, row.Offer.Selection.Fields.Select(field => field.Name));
            Assert.Equal(BackofficeAncillaryOfferMapper.DisplayStateOf(row.Candidate.OfferReadiness).ToString(), row.Offer.DisplayState.Name);
            Assert.Equal(row.Candidate.Price.CompleteUnitTotal, row.Offer.Pricing.CompleteUnitTotal);
            Assert.Equal(row.Candidate.Quantity.Unit.ToString(), row.Offer.Quantity.Unit.Name);
            Assert.False(row.Offer.CanPurchase);
            Assert.False(row.Offer.Availability.IsGuaranteed);
        });
    }

    [Theory]
    [InlineData(OfferReadiness.Selectable, AncillaryOfferDisplayState.AvailableForSelection, true)]
    [InlineData(OfferReadiness.NeedsSelection, AncillaryOfferDisplayState.NeedsInput, true)]
    [InlineData(OfferReadiness.NeedsQuote, AncillaryOfferDisplayState.QuoteRequired, true)]
    [InlineData(OfferReadiness.NeedsVerification, AncillaryOfferDisplayState.NeedsVerification, false)]
    [InlineData(OfferReadiness.Unavailable, AncillaryOfferDisplayState.Unavailable, false)]
    [InlineData(OfferReadiness.NotEligible, AncillaryOfferDisplayState.NotEligible, false)]
    public void CT_20_the_display_state_is_one_translation_of_the_readiness(OfferReadiness readiness, AncillaryOfferDisplayState state, bool discoverable)
    {
        Assert.Equal((state, discoverable), (BackofficeAncillaryOfferMapper.DisplayStateOf(readiness), BackofficeAncillaryOfferMapper.IsDiscoverable(readiness)));
        Assert.Equal(6, Enum.GetValues<OfferReadiness>().Length);
        Assert.Equal(Enum.GetValues<OfferReadiness>().Length, Enum.GetValues<OfferReadiness>().Select(BackofficeAncillaryOfferMapper.DisplayStateOf).Distinct().Count());
        Assert.Throws<ArgumentOutOfRangeException>(() => BackofficeAncillaryOfferMapper.DisplayStateOf((OfferReadiness)99));
    }

    [Fact]
    public async Task CT_04_the_points_of_sale_are_the_named_one_or_the_whole_eligible_set_and_never_a_default()
    {
        var named = new Eligibility();
        var explicitScope = await Resolver(eligibility: named).ResolveAsync(new BackofficeAncillaryOfferOptions { PointOfSaleId = 77 });
        var single = new Eligibility { Result = InventoryReferenceCheck.Verified, Allowed = [88] };
        var resolvedScope = await Resolver(eligibility: single).ResolveAsync(new BackofficeAncillaryOfferOptions { CountryId = 5 });

        Assert.Equal((Airline, Channel.BackOffice, 77L), (explicitScope.OwnerAirlineId, explicitScope.Channel, explicitScope.PointOfSaleIds.Single()));
        Assert.Null(named.Asked);
        Assert.Equal((Airline, Channel.BackOffice, 88L), (resolvedScope.OwnerAirlineId, resolvedScope.Channel, resolvedScope.PointOfSaleIds.Single()));
        Assert.Equal((Channel.BackOffice, 5L, 9200L, (long?)null), (single.Asked!.Channel, single.Asked.CountryId, single.Asked.AirlineOfficeIds.Single(), single.Asked.TravelAgencyOfficeId));

        Assert.Empty((await Resolver(eligibility: new Eligibility { Result = InventoryReferenceCheck.Verified }).ResolveAsync(null)).PointOfSaleIds);
        Assert.Equal(
            new long[] { 1, 2 },
            (await Resolver(eligibility: new Eligibility { Result = InventoryReferenceCheck.Verified, Allowed = [2, 1, 2] }).ResolveAsync(null)).PointOfSaleIds);

        foreach (var unresolved in new[] { new Eligibility(), new Eligibility { Result = InventoryReferenceCheck.NotFound, Allowed = [1] } })
        {
            var failure = await Assert.ThrowsAsync<BusinessException>(() => Resolver(eligibility: unresolved).ResolveAsync(null));

            Assert.Equal((16808, 409), (failure.Code, failure.HttpStatus));
            Assert.StartsWith("POS_CONTEXT_UNRESOLVED", failure.Message, StringComparison.Ordinal);
            Assert.Contains(AuthorizedPointsOfSale.BlockedExternalReference, failure.Message, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task CT_02_CT_21_only_an_airline_user_of_an_office_of_the_home_airline_gets_a_sales_scope()
    {
        var options = new BackofficeAncillaryOfferOptions { PointOfSaleId = 77 };

        foreach (var resolver in new[]
                 {
                     Resolver(new Caller { ContextType = CallerContextType.TravelAgency }),
                     Resolver(new Caller { ContextType = CallerContextType.None }),
                     Resolver(new Caller { AirlineOfficeId = null }),
                     Resolver(new Caller { AirlineUserId = null }),
                     Resolver(home: new HomeAirline { AirlineId = null }),
                     Resolver(home: new HomeAirline { AirlineId = 0 })
                 })
        {
            var failure = await Assert.ThrowsAsync<BusinessException>(() => resolver.ResolveAsync(options));

            Assert.Equal((16809, 403), (failure.Code, failure.HttpStatus));
        }
    }

    [Fact]
    public async Task CT_22_an_empty_offer_id_is_refused_before_the_provider_is_called()
    {
        var failure = await Assert.ThrowsAsync<BusinessException>(
            () => new GetAncillaryOffersService(new FlightOfferShoppingSource(new FixedOffers(null!), new TestOnlyAirportReference(), new TestOnlyAirFareReference()), new ShoppingLab().Engine).ExecuteAsync(new OfferQuery(" "), Sale));

        Assert.Equal((16803, 422), (failure.Code, failure.HttpStatus));
        Assert.Equal(new[] { "OfferId", "Options" }, typeof(BackofficeGetAncillaryOffersQuery).GetProperties().Select(property => property.Name).Order());
        Assert.Equal(
            new[] { "CountryId", "PointOfSaleId" },
            typeof(BackofficeAncillaryOfferOptions).GetProperties().Select(property => property.Name).Order());
    }
}
