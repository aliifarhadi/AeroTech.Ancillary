using System.Text.Json;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Providers.Offer.Wire;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
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
using PassengerTypeCode = AeroTech.Messages.AirPrice.Enums.PassengerTypeCode;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S201ContextCoverageTests
{
    private static readonly AncillarySalesScope Scope = new(Airline, Channel.BackOffice, [Pos]);

    [Fact]
    public async Task S201_SRC_01_the_verified_producer_fields_survive_the_mapping_per_traveller_and_per_flight()
    {
        var offer = await ReadAsync(TwoBoundOffer());
        var facts = FlightOfferShoppingContextMapper.Facts(offer, new Dictionary<long, long> { [9001] = 4, [9002] = 2 });
        var flights = offer.Bounds.SelectMany(bound => bound.Flights).ToList();

        Assert.Equal(new[] { (501L, 8501L), (502L, 8502L), (503L, 8503L), (504L, 8504L) }, flights.Select(flight => (flight.FlightId, flight.FlightCapacityId)));
        Assert.All(flights, flight => Assert.NotEqual(flight.FlightId, flight.FlightCapacityId));
        Assert.Equal([2, 3, 2, 3], flights.Select(flight => flight.FlightVersion));
        Assert.Equal(new[] { "T1", "T2" }, offer.Tickets.Select(ticket => ticket.TravellerRef));
        Assert.Equal(new[] { PassengerTypeCode.ADT, PassengerTypeCode.CHD }, offer.Tickets.Select(ticket => ticket.PassengerTypeCode));
        Assert.All(offer.Tickets[0].Coupons, coupon => Assert.Equal((true, true, false), (coupon.IsRefundable, coupon.IsChangeable, coupon.IsUpgradable)));
        Assert.Equal((1, 30m), (offer.Tickets[0].Coupons[0].CheckedBaggage!.Pieces, offer.Tickets[0].Coupons[0].CheckedBaggage!.Weight));
        Assert.Equal((1, 7m), (offer.Tickets[0].Coupons[0].CabinBaggage!.Pieces, offer.Tickets[0].Coupons[0].CabinBaggage!.Weight));
        Assert.Equal(("ROE-1", 155, 70, 1_050_000m), (offer.RatesOfExchange[0].RateOfExchangePeriodId, offer.RatesOfExchange[0].FromCurrencyId, offer.RatesOfExchange[0].ToCurrencyId, offer.RatesOfExchange[0].Rate));
        Assert.Equal((FlightOfferPriceCategory.Tax, 9m, 70), (offer.OrderCharges[0].Category, offer.OrderCharges[0].Amount, offer.OrderCharges[0].CurrencyId));
        Assert.Equal(120m, offer.Tickets[0].Coupons[0].PriceLines.Single(line => line.Category == FlightOfferPriceCategory.Fare).Amount);
        Assert.Equal(8, facts.Fares.Count);
        Assert.All(offer.Tickets, ticket => Assert.Equal(4, facts.Fares.Count(fact => fact.TravellerRef == ticket.TravellerRef)));
        Assert.All(facts.Fares.Where(fact => fact.PortionRef == "B1"), fact => Assert.Equal((9001L, "YFLEX", "Flex", 4L), (fact.AirFareId, fact.FareBasisCode, fact.FareFamilyCodeOrName, fact.FareFamilyId)));
        Assert.All(facts.Fares.Where(fact => fact.PortionRef == "B2"), fact => Assert.Equal((9002L, "YLITE", "Light", 2L), (fact.AirFareId, fact.FareBasisCode, fact.FareFamilyCodeOrName, fact.FareFamilyId)));
        Assert.Equal(8, facts.Baggage.Count);
        Assert.True(facts.Completeness is { FareFactsComplete: true, BaggageFactsComplete: true });
    }

    [Fact]
    public async Task S201_SRC_02_a_source_without_a_flight_capacity_or_with_a_wrong_coupon_is_refused_as_inconsistent()
    {
        var noCapacity = TwoBoundOffer();

        noCapacity.AirTransports[0].Flights[0].FlightCapacityId = 0;

        var missingFlight = TwoBoundOffer();

        missingFlight.Tickets[0].Coupons[0].FlightId = 999;

        var tamperedRate = TwoBoundOffer();

        tamperedRate.RatesOfExchange[0].Rate = 0m;

        foreach (var broken in new[] { noCapacity, missingFlight, tamperedRate })
        {
            var refused = await Assert.ThrowsAsync<BusinessException>(() => ReadAsync(broken));

            Assert.Equal((16807, 502), (refused.Code, refused.HttpStatus));
        }
    }

    [Fact]
    public async Task S201_CTX_01_the_evaluation_clock_is_the_server_clock_and_validity_is_the_configured_ttl_never_the_sales_or_ticketing_date()
    {
        var offer = await OfferWire.ReadAsync(OfferWire.Captured());
        var source = new FlightOfferShoppingSource(
            new FixedOffers(offer),
            new TestOnlyAirportReference(),
            new TestOnlyAirFareReference(),
            new FixedClock { Now = Now },
            new FlightOfferShoppingOptions(SourceContextTtl));
        var shopping = await source.LoadAsync(OfferWire.OfferId, Scope);
        var context = shopping.PointsOfSale.Single().Context;

        Assert.NotEqual(Now, offer.PricedAt.ToUniversalTime());
        Assert.Equal(Now, context.EvaluatedAtUtc);
        Assert.Equal(Now + SourceContextTtl, context.SourceIdentity.ValidUntilUtc);
        Assert.NotEqual(offer.LastTicketingDate, context.SourceIdentity.ValidUntilUtc);
        Assert.Equal(FlightOfferFingerprint.Of(offer), context.SourceIdentity.TrustedSourceVersion);
    }

    [Fact]
    public async Task S201_CTX_02_the_backoffice_answer_states_the_server_evaluation_clock_and_keeps_the_sales_date_as_a_source_fact()
    {
        var offer = (await OfferWire.ReadAsync(OfferWire.Captured())) with { SalesChannel = Channel.BackOffice };
        var service = new GetAncillaryOffersService(
            new FlightOfferShoppingSource(
                new FixedOffers(offer),
                new TestOnlyAirportReference(),
                new TestOnlyAirFareReference(),
                new FixedClock { Now = Now },
                new FlightOfferShoppingOptions(SourceContextTtl)),
            new ShoppingLab().Engine);
        var answer = BackofficeAncillaryOffersMapper.Map(await service.ExecuteAsync(new OfferQuery(OfferWire.OfferId), Scope));

        Assert.Equal(Now, answer.EvaluatedAtUtc);
        Assert.Equal(offer.PricedAt, answer.FlightOffer.SourcePricedAt);
        Assert.NotEqual(answer.EvaluatedAtUtc, answer.FlightOffer.SourcePricedAt);
    }

    [Fact]
    public async Task S201_FPR_01_the_fingerprint_changes_with_the_flight_version_the_aircraft_and_the_priced_amounts_and_only_with_them()
    {
        var baseline = FlightOfferFingerprint.Of(await ReadAsync(TwoBoundOffer()));
        var same = FlightOfferFingerprint.Of(await ReadAsync(TwoBoundOffer()));

        var reflown = TwoBoundOffer();

        reflown.AirTransports[0].Flights[0].FlightVersion = 9;

        var swapped = TwoBoundOffer();

        swapped.AirTransports[0].Flights[0].AircraftId = 4;

        var repriced = TwoBoundOffer();

        repriced.TotalAmount = 999m;

        Assert.Equal(baseline, same);

        foreach (var changed in new[] { reflown, swapped, repriced })
            Assert.NotEqual(baseline, FlightOfferFingerprint.Of(await ReadAsync(changed)));
    }

    [Fact]
    public async Task S201_ORD_01_an_order_scoped_product_is_offered_exactly_once_without_beneficiaries_and_priced_once()
    {
        var lab = new ShoppingLab();
        var wifi = lab.Definition("A24");
        var provision = lab.Provision(wifi, scope: ServiceCoverageScope.Order);

        lab.Price(wifi, provision, Rate(100m));
        lab.Policy(wifi, InventoryAuthority.Supplier);
        lab.Sellable("A01");

        var context = Trip.Context(travellers: [Trip.Adult("T1"), Trip.Child("T2")]);
        var result = await lab.Engine.ShopAsync(context);
        var order = Assert.Single(result.Candidates, candidate => candidate.VariantCode == "A24");
        var bags = result.Candidates.Where(candidate => candidate.VariantCode == "A01").ToList();

        Assert.Equal(ServiceCoverageScope.Order, order.CandidateIdentity.CoverageScope);
        Assert.Equal(string.Empty, order.CandidateIdentity.TravellerRef);
        Assert.Equal("ORDER", order.CandidateIdentity.CoverageRef);
        Assert.Empty(order.Travellers);
        Assert.Equal(100m, order.Price.CompleteUnitTotal);
        Assert.NotEmpty(bags);
        Assert.All(bags, bag => Assert.Single(bag.Travellers));
        Assert.Equal(new[] { "T1", "T2" }, bags.Select(bag => bag.Travellers.Single()).Distinct().Order());
    }

    [Fact]
    public async Task S201_ORD_04_an_order_scoped_airport_package_is_one_candidate_for_the_whole_order_on_a_priced_offer()
    {
        var lab = new ShoppingLab();
        var cip = lab.Definition("A22");
        var provision = lab.Provision(cip, scope: ServiceCoverageScope.Order);

        lab.Price(cip, provision, Rate(250m));

        var context = Trip.Context([Trip.Flight(origin: Trip.Ika)], travellers: [Trip.Adult("T1"), Trip.Child("T2")]);
        var result = await lab.Engine.ShopAsync(context);
        var order = Assert.Single(result.Candidates, candidate => candidate.VariantCode == "A22");

        Assert.Equal((ServiceCoverageScope.Order, "", "ORDER"), (order.CandidateIdentity.CoverageScope, order.CandidateIdentity.TravellerRef, order.CandidateIdentity.CoverageRef));
        Assert.Empty(order.Travellers);
        Assert.Equal(250m, order.Price.CompleteUnitTotal);
    }

    [Fact]
    public async Task S201_ORD_02_personal_products_and_passenger_bound_rules_never_match_an_order_unit()
    {
        var lab = new ShoppingLab();
        var wheelchair = lab.Definition("A15");
        var seat = lab.Definition("A07");
        var restricted = lab.Definition("A24", reference: "WIFI_ADT_ONLY");

        lab.Provision(wheelchair, scope: ServiceCoverageScope.Order);
        lab.Provision(seat, scope: ServiceCoverageScope.Order);

        var provision = lab.Provision(
            restricted,
            scope: ServiceCoverageScope.Order,
            rules: Rules(passengers: new ProvisionPassengerEligibilityArgs([PassengerTypeCode.ADT], [])));

        lab.Price(restricted, provision, Rate(50m));

        var result = await lab.Engine.ShopAsync(Trip.Context(), new ShoppingFilter { IncludeNonSellable = true });

        Assert.DoesNotContain(result.Candidates, candidate => candidate.VariantCode is "A15" or "A07");
        Assert.DoesNotContain(result.Candidates, candidate => candidate.ServiceDefinitionRef == "WIFI_ADT_ONLY");
        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Code == ShoppingReasonCodes.NoMatchingProvision && diagnostic.MissingContextFields.Contains("WIFI_ADT_ONLY"));
    }

    [Fact]
    public async Task S201_ORD_03_an_order_candidate_is_evaluated_and_accepted_without_a_traveller()
    {
        var lab = new ShoppingLab();
        var wifi = lab.Definition("A24");
        var provision = lab.Provision(wifi, scope: ServiceCoverageScope.Order);

        lab.Price(wifi, provision, Rate(100m));
        lab.Policy(wifi, InventoryAuthority.Supplier);

        var context = Trip.Context(travellers: [Trip.Adult("T1"), Trip.Child("T2")]);
        var shown = (await lab.Engine.ShopAsync(context)).Candidates.Single(candidate => candidate.VariantCode == "A24");
        var evaluation = await lab.Engine.EvaluateSelectionAsync(context, shown.CandidateIdentity, new AncillarySelection { PlanCode = "WIFI_FULL_FLIGHT", DeviceCount = 1 });

        Assert.Equal(SelectionEvaluationStatus.Accepted, evaluation.Status);
        Assert.Equal(string.Empty, evaluation.Candidate.CandidateIdentity.TravellerRef);
        Assert.Empty(evaluation.Candidate.Travellers);
    }

    private static FlightOfferDetailResponse TwoBoundOffer()
    {
        OfferFlight Flight(int sequence, long flightId, int version, int origin, int destination)
            => new()
            {
                Sequence = sequence,
                FlightCapacityId = 8000 + flightId,
                FlightId = flightId,
                FlightVersion = version,
                FlightNumber = "111",
                OriginAirportId = origin,
                DestinationAirportId = destination,
                OperatingAirlineId = 1,
                MarketingAirlineId = 1,
                DepartureDateTime = Now.AddDays(30).AddHours(sequence),
                ArrivalDateTime = Now.AddDays(30).AddHours(sequence + 2),
                AircraftId = 3,
                CabinClassId = 1,
                RbdId = 11,
                BookingClass = "Y",
                Legs = []
            };

        OfferCoupon Coupon(string boundId, long flightId, long airFareId, bool upgradable)
            => new()
            {
                BoundId = boundId,
                FlightId = flightId,
                BaggagePieces = 1,
                BaggageWeight = 30,
                BaggageUnit = "Kg",
                CabinBaggagePieces = 1,
                CabinBaggageWeight = 7,
                CabinBaggageUnit = "Kg",
                IsRefundable = true,
                IsChangeable = true,
                IsUpgradable = upgradable,
                Pricings =
                [
                    new OfferPricingLine { Category = OfferPricingCategory.Fare, Reference = airFareId.ToString(), Amount = 120m, CurrencyId = 70, Name = "FARE" },
                    new OfferPricingLine { Category = OfferPricingCategory.Tax, Code = "IR", Amount = 6m, CurrencyId = 70 }
                ]
            };

        OfferTicket Ticket(string reference, int index, string passengerType, bool upgradable)
            => new()
            {
                TravellerRef = reference,
                TravellerIndex = index,
                PassengerTypeCode = passengerType,
                Coupons =
                [
                    Coupon("B1", 501, 9001, upgradable),
                    Coupon("B1", 502, 9001, upgradable),
                    Coupon("B2", 503, 9002, upgradable),
                    Coupon("B2", 504, 9002, upgradable)
                ]
            };

        return new FlightOfferDetailResponse
        {
            OfferId = OfferWire.OfferId,
            PricedAt = Now.AddDays(-2),
            LastTicketingDate = Now.AddDays(5),
            CurrencyId = 70,
            CurrencyCode = "IRR",
            BaseAmount = 480m,
            ChargeAmount = 57m,
            TotalAmount = 537m,
            AirTransports =
            [
                new OfferAirTransport
                {
                    BoundId = "B1",
                    BoundOfferId = "BO1",
                    Direction = OfferBoundDirection.Outbound,
                    Sequence = 1,
                    OriginAirportId = 2,
                    DestinationAirportId = 15,
                    Flights = [Flight(1, 501, 2, 2, 6), Flight(2, 502, 3, 6, 15)]
                },
                new OfferAirTransport
                {
                    BoundId = "B2",
                    BoundOfferId = "BO2",
                    Direction = OfferBoundDirection.Inbound,
                    Sequence = 2,
                    OriginAirportId = 15,
                    DestinationAirportId = 2,
                    Flights = [Flight(1, 503, 2, 15, 6), Flight(2, 504, 3, 6, 2)]
                }
            ],
            PricingUnits =
            [
                new OfferPricingUnit
                {
                    CoveredBoundOfferIds = ["BO1", "BO2"],
                    FareComponents =
                    [
                        new OfferFareComponent { AirFareId = 9001, BoundId = "B1", CabinClassId = 1, RbdId = 11, BookingClass = "Y", FareBasis = "YFLEX", FareFamily = "Flex", FareType = "Public" },
                        new OfferFareComponent { AirFareId = 9002, BoundId = "B2", CabinClassId = 1, RbdId = 11, BookingClass = "Y", FareBasis = "YLITE", FareFamily = "Light", FareType = "Public" }
                    ]
                }
            ],
            Tickets = [Ticket("T1", 1, "ADT", false), Ticket("T2", 2, "CHD", true)],
            OrderCharges = [new OfferPricingLine { Category = OfferPricingCategory.Tax, Code = "IR", Amount = 9m, CurrencyId = 70 }],
            RatesOfExchange = [new OfferRateOfExchange { RateOfExchangePeriodId = "ROE-1", FromCurrencyId = 155, ToCurrencyId = 70, Rate = 1_050_000m, DecimalPlaces = 0, RoundingFactor = 1000 }]
        };
    }

    private static Task<FlightOffer> ReadAsync(FlightOfferDetailResponse data)
        => OfferWire.ReadAsync(JsonSerializer.Serialize(new OfferEnvelope<FlightOfferDetailResponse> { Data = data }, OfferJson.Options));
}
