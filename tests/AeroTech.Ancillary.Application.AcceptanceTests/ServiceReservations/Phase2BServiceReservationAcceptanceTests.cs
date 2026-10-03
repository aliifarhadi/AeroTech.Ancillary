using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.CancelServiceReservationUnits;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ConfirmServiceReservation;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReleaseServiceReservation;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation.Service;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Contracts;
using AeroTech.Ancillary.Query.ServiceReservationAggregate.Queries.GetServiceReservationById;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.ServiceReservations;

[Collection(DatabaseCollection.Name)]
public sealed class Phase2BServiceReservationAcceptanceTests(TestDatabase database) : IAsyncLifetime
{
    private readonly AncillaryHarness _harness = new(database);

    private AncillaryProductResult _productX = null!;
    private AncillaryPriceRuleResult _ruleR = null!;
    private AncillaryPriceRuleResult _ruleRG = null!;
    private AncillaryPriceRuleResult _ruleRL = null!;

    private int Airline => _harness.AirlineId;

    private string KeyPrefix => $"p2b-{Airline}:";

    public async Task InitializeAsync()
    {
        await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));
        await _harness.RegisterAsync(Phase1Commands.SubCodeG(Airline));
        await _harness.RegisterAsync(Phase2Commands.SubCodeL(Airline));

        _productX = await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));
        await _harness.ArrangeProductAsync(Phase1Commands.ProductG(Airline));
        await _harness.ArrangeProductAsync(Phase2Commands.ProductL(Airline));

        _ruleR = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));
        _ruleRG = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleRG(Airline));
        _ruleRL = await _harness.ArrangePriceRuleAsync(Phase2Commands.RuleRL(Airline));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task P2B_V01_ReserveHoldsOneUnitWithTheAcceptedValues()
    {
        var result = await _harness.ReserveAsync(ReserveX("v01"));

        var unit = Assert.Single(result.Units);

        Assert.Equal((Key("v01"), "order:9001"), (result.IdempotencyKey, result.Reference));
        Assert.Equal(
            (ServiceReservationUnitStatus.Held, "U1", Airline, "XBAG1", 1, _ruleR.Id, "T1", "B1", (string?)null),
            (unit.Status, unit.UnitReference, unit.OwnerAirlineId, unit.ProductRef, unit.ProductVersion, unit.PriceRuleId, unit.TravellerRef, unit.BoundRef, unit.FlightRef));
        Assert.Equal([Phase2BCommands.FlightId], unit.CoveredFlightIds);
        Assert.Equal((1, Phase1Commands.CurrencyId, 38.50m, AncillaryInventoryControl.Unlimited), (unit.Quantity, unit.CurrencyId, unit.Total, unit.InventoryControl));

        var stored = await _harness.RunAsync(scope => scope.Command.ServiceReservations
            .AsNoTracking()
            .Include(reservation => reservation.Units)
            .SingleAsync(reservation => reservation.Id == result.ReservationId));

        Assert.Equal(unit.UnitRef, Assert.Single(stored.Units).Id);
        Assert.Equal((ServiceReservationUnitStatus.Held, 38.50m), (stored.Units.Single().Status, stored.Units.Single().Total));
    }

    [Fact]
    public async Task P2B_V01_ReservationIsWrittenInTheDocumentedShape()
    {
        var command = ReserveX("v01-shape");
        var result = await _harness.ReserveAsync(command);
        var unit = Assert.Single(result.Units);

        var expected = ApiJson.With(
            $$"""
            {
              "reservationId": "{{result.ReservationId}}", "idempotencyKey": "{{Key("v01-shape")}}", "reference": "order:9001",
              "expiresAt": null,
              "units": [
                { "unitRef": "{{unit.UnitRef}}", "unitReference": "U1", "status": "Held",
                  "ownerAirlineId": {{Airline}}, "productRef": "XBAG1", "productVersion": 1, "priceRuleId": "{{_ruleR.Id}}",
                  "travellerRef": "T1", "boundRef": "B1", "flightRef": null, "coveredFlightIds": ["100"],
                  "quantity": 1, "currencyId": 978, "total": 38.50, "inventoryControl": "Unlimited" }
              ]
            }
            """,
            "expiresAt",
            JsonNode.Parse(JsonSerializer.Serialize(command.ExpiresAt, ApiJson.Options)));

        ApiJson.AssertSame(JsonNode.Parse(expected), JsonNode.Parse(JsonSerializer.Serialize(result, ApiJson.Options)));
    }

    [Fact]
    public void P2B_V01_DocumentedReserveRequestIsAccepted()
    {
        var command = ApiJson.Deserialize<ServiceReserveServiceReservationCommand>("""
            {
              "idempotencyKey": "reserve-hold:9300",
              "reference": "order:9001",
              "expiresAt": "2026-10-03T12:30:00+00:00",
              "context": {
                "currencyId": 978,
                "asOf": "2026-10-03T12:00:00+00:00",
                "salesContext": { "channel": "BackOffice", "customerId": null, "travelAgencyId": null, "countryId": null },
                "travellers": [ { "ref": "7001", "passengerTypeCode": "ADT", "flightRefs": ["8101"] } ],
                "bounds": [ { "ref": "8001", "flights": [
                  { "ref": "8101", "flightId": "100", "originAirportId": 1, "destinationAirportId": 2,
                    "departureDateTime": "2026-10-10T08:00:00+03:30", "marketingAirlineId": 10, "operatingAirlineId": 10 } ] } ],
                "existing": []
              },
              "units": [
                { "unitReference": "9102", "productRef": "XBAG1", "productVersion": 1, "priceRuleId": "501",
                  "travellerRef": "7001", "boundRef": "8001", "flightRef": null, "quantity": 1 }
              ]
            }
            """);

        ValidationAssert.Accepts(new ServiceReserveServiceReservationCommandValidator(), command);
        Assert.Equal(100, Assert.Single(Assert.Single(command.Context.Bounds).Flights).FlightId);
        Assert.Equal(("9102", 501L, "8001", (string?)null), (command.Units[0].UnitReference, command.Units[0].PriceRuleId, command.Units[0].BoundRef, command.Units[0].FlightRef));
    }

    [Fact]
    public async Task P2B_V02_RepeatedReserveReturnsTheSameReservationWithoutEvaluating()
    {
        var command = ReserveX("v02");
        var first = await _harness.ReserveAsync(command);
        var again = await _harness.ReserveAsync(command);

        Assert.Equal((first.ReservationId, first.Units[0].UnitRef), (again.ReservationId, again.Units[0].UnitRef));

        await _harness.RetirePriceRuleAsync(_ruleR.Id);
        await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(40.00m)] });

        var replayed = await _harness.ReserveAsync(command);

        Assert.Equal(Serialized(first), Serialized(replayed));
        Assert.Equal((1, 1), (await StoredReservationsAsync(), await StoredUnitsAsync()));
    }

    [Fact]
    public async Task P2B_V03_SameKeyWithOtherContentIsRefusedAndOnlyTheContextIsNotCompared()
    {
        var command = ReserveX("v03");
        var held = await _harness.ReserveAsync(command);

        await BusinessAssert.ThrowsAsync(16405, 409, () => _harness.ReserveAsync(command with { Units = [Phase2BCommands.BagUnit(_ruleR.Id, quantity: 2)] }));
        await BusinessAssert.ThrowsAsync(16405, 409, () => _harness.ReserveAsync(command with { ExpiresAt = command.ExpiresAt.AddMinutes(1) }));
        await BusinessAssert.ThrowsAsync(16405, 409, () => _harness.ReserveAsync(command with { Reference = "order:9002" }));
        await BusinessAssert.ThrowsAsync(16405, 409, () => _harness.ReserveAsync(command with { Units = [Phase2BCommands.BagUnit(_ruleR.Id), Phase2BCommands.LoungeUnit(_ruleRL.Id)] }));
        await BusinessAssert.ThrowsAsync(16405, 409, () => _harness.ReserveAsync(command with { Units = [Phase2BCommands.BagUnit(_ruleR.Id, "U9")] }));
        await BusinessAssert.ThrowsAsync(16405, 409, () => _harness.ReserveAsync(command with { Units = [Phase2BCommands.BagUnit(_ruleRG.Id)] }));

        var otherContext = command with
        {
            Context = command.Context with { CurrencyId = 840, AsOf = command.Context.AsOf.AddDays(-30), Existing = [Phase2BCommands.Existing("XBAG1")] }
        };
        var replayed = await _harness.ReserveAsync(otherContext);

        Assert.Equal(Serialized(held), Serialized(replayed));
        Assert.Equal((1, 1), (await StoredReservationsAsync(), await StoredUnitsAsync()));
    }

    [Fact]
    public async Task P2B_V04_UnitFailingTheSelectionChecksIsRefusedWithThatCodeAndNothingIsStored()
    {
        var context = Context();
        var twoFlightBound = context with
        {
            Travellers = [new ReservationTravellerInput("T1", "ADT", ["F1", "F1B"])],
            Bounds = [new ReservationBoundInput("B1", [Phase2BCommands.Flight("F1", 100, Airline), Phase2BCommands.Flight("F1B", 101, Airline) with { OriginAirportId = 2, DestinationAirportId = 3 }])]
        };
        var repeatedTraveller = context with
        {
            Travellers = [new ReservationTravellerInput("T1", "ADT", ["F1"]), new ReservationTravellerInput("T1", "ADT", ["F1"])]
        };
        var bag = Phase2BCommands.BagUnit(_ruleR.Id);

        await BusinessAssert.ThrowsAsync(16302, 422, () => _harness.ReserveAsync(Reserve("v04-a", context, bag with { ProductRef = "XBAGZ" })));
        await BusinessAssert.ThrowsAsync(16309, 409, () => _harness.ReserveAsync(Reserve("v04-b", context, bag with { ProductVersion = 2 })));
        await BusinessAssert.ThrowsAsync(16309, 409, () => _harness.ReserveAsync(Reserve("v04-c", context, bag with { PriceRuleId = _ruleRG.Id })));
        await BusinessAssert.ThrowsAsync(16305, 422, () => _harness.ReserveAsync(Reserve("v04-d", twoFlightBound, bag)));
        await BusinessAssert.ThrowsAsync(16306, 422, () => _harness.ReserveAsync(Reserve("v04-e", context, bag with { Quantity = 2 })));
        await BusinessAssert.ThrowsAsync(16303, 422, () => _harness.ReserveAsync(Reserve("v04-f", context, bag with { FlightRef = "F1" })));
        await BusinessAssert.ThrowsAsync(16304, 422, () => _harness.ReserveAsync(Reserve("v04-g", context, bag with { TravellerRef = "T9" })));
        await BusinessAssert.ThrowsAsync(16301, 422, () => _harness.ReserveAsync(Reserve("v04-h", repeatedTraveller, bag)));

        Assert.Equal((0, 0), (await StoredReservationsAsync(), await StoredUnitsAsync()));
    }

    [Fact]
    public async Task P2B_V05_SecondUnitFailingRefusesTheWholeRequest()
    {
        var failing = Phase2BCommands.LoungeUnit(_ruleRL.Id) with { FlightRef = "F9" };

        await BusinessAssert.ThrowsAsync(16304, 422, () => _harness.ReserveAsync(Reserve("v05", Context(), Phase2BCommands.BagUnit(_ruleR.Id), failing)));

        Assert.Equal((0, 0), (await StoredReservationsAsync(), await StoredUnitsAsync()));
    }

    [Fact]
    public async Task P2B_V06_TwoUnitsNamingTheSameOccurrenceAreRefused()
    {
        await BusinessAssert.ThrowsAsync(
            16307,
            422,
            () => _harness.ReserveAsync(Reserve("v06", Context(), Phase2BCommands.BagUnit(_ruleR.Id), Phase2BCommands.BagUnit(_ruleR.Id, "U2"))));

        Assert.Equal((0, 0), (await StoredReservationsAsync(), await StoredUnitsAsync()));
    }

    [Fact]
    public async Task P2B_V07_ExistingEntriesOfTheOrderCountAgainstTheQuantity()
    {
        await BusinessAssert.ThrowsAsync(
            16305,
            422,
            () => _harness.ReserveAsync(Reserve("v07-x", Context(Phase2BCommands.Existing("XBAG1")), Phase2BCommands.BagUnit(_ruleR.Id))));

        var generic = await _harness.ReserveAsync(Reserve("v07-g1", Context(Phase2BCommands.Existing("XBAGG")), Phase2BCommands.GenericBagUnit(_ruleRG.Id, 1)));

        Assert.Equal((ServiceReservationUnitStatus.Held, 30.00m), (Assert.Single(generic.Units).Status, generic.Units[0].Total));

        await BusinessAssert.ThrowsAsync(
            16306,
            422,
            () => _harness.ReserveAsync(Reserve("v07-g2", Context(Phase2BCommands.Existing("XBAGG")), Phase2BCommands.GenericBagUnit(_ruleRG.Id, 2))));

        Assert.Equal((1, 1), (await StoredReservationsAsync(), await StoredUnitsAsync()));
    }

    [Fact]
    public async Task P2B_V08_LoungeUnitIsHeldOnItsFlightAndBound()
    {
        var command = Reserve("v08", Context(), Phase2BCommands.LoungeUnit(_ruleRL.Id, "U1"));
        var result = await _harness.ReserveAsync(command);

        var unit = Assert.Single(result.Units);

        Assert.Equal(
            (ServiceReservationUnitStatus.Held, "LNGTHR", 1, _ruleRL.Id, "T1", "B1", (string?)"F1", 20.00m),
            (unit.Status, unit.ProductRef, unit.ProductVersion, unit.PriceRuleId, unit.TravellerRef, unit.BoundRef, unit.FlightRef, unit.Total));
        Assert.Equal([Phase2BCommands.FlightId], unit.CoveredFlightIds);
        Assert.Equal(result.ReservationId, (await _harness.ReserveAsync(command)).ReservationId);
    }

    [Fact]
    public async Task P2B_V09_BagAndLoungeAreHeldInOneReservationInRequestOrder()
    {
        var result = await _harness.ReserveAsync(Reserve("v09", Context(), Phase2BCommands.LoungeUnit(_ruleRL.Id, "U1"), Phase2BCommands.BagUnit(_ruleR.Id, "U2")));
        var read = await _harness.GetReservationAsync(result.ReservationId);

        Assert.Equal([("U1", "LNGTHR"), ("U2", "XBAG1")], result.Units.Select(unit => (unit.UnitReference, unit.ProductRef)));
        Assert.Equal([("U1", "LNGTHR"), ("U2", "XBAG1")], read.Units.Select(unit => (unit.UnitReference, unit.ProductRef)));
        Assert.Equal((1, 2), (await StoredReservationsAsync(), await StoredUnitsAsync()));
    }

    [Fact]
    public async Task P2B_V10_InvalidReservationRequestsAreRefused()
    {
        var bag = Phase2BCommands.BagUnit(_ruleR.Id);
        var now = _harness.Clock.Now;

        await BusinessAssert.ThrowsAsync(16412, 422, () => _harness.ReserveAsync(Phase2BCommands.Reserve(Key("v10-a"), now, Context(), bag)));
        await BusinessAssert.ThrowsAsync(16412, 422, () => _harness.ReserveAsync(Phase2BCommands.Reserve(Key("v10-b"), now.AddTicks(-1), Context(), bag)));
        await BusinessAssert.ThrowsAsync(16412, 422, () => _harness.ReserveAsync(Reserve("v10-c", Context(), bag, Phase2BCommands.LoungeUnit(_ruleRL.Id, "U1"))));
        await BusinessAssert.ThrowsAsync(16412, 422, () => _harness.ReserveAsync(Reserve("v10-d", Context())));

        Assert.Equal((0, 0), (await StoredReservationsAsync(), await StoredUnitsAsync()));
    }

    [Fact]
    public void P2B_V10_MissingKeyOrReferenceIsAMalformedRequest()
    {
        var validator = new ServiceReserveServiceReservationCommandValidator();
        var command = ReserveX("v10-shape");

        ValidationAssert.Accepts(validator, command);
        ValidationAssert.Accepts(validator, command with { Units = [] });
        ValidationAssert.Accepts(validator, command with { ExpiresAt = _harness.Clock.Now.AddDays(-1) });
        ValidationAssert.Rejects(validator, command with { IdempotencyKey = null! });
        ValidationAssert.Rejects(validator, command with { IdempotencyKey = "" });
        ValidationAssert.Rejects(validator, command with { IdempotencyKey = new string('k', 129) });
        ValidationAssert.Rejects(validator, command with { Reference = null! });
        ValidationAssert.Rejects(validator, command with { Reference = "" });
        ValidationAssert.Rejects(validator, command with { Reference = new string('r', 129) });
        ValidationAssert.Rejects(validator, command with { ExpiresAt = default });
        ValidationAssert.Rejects(validator, command with { Units = null! });
        ValidationAssert.Rejects(validator, command with { Units = [Phase2BCommands.BagUnit(_ruleR.Id, "")] });
        ValidationAssert.Rejects(validator, command with { Units = [Phase2BCommands.BagUnit(_ruleR.Id, new string('u', 129))] });
        ValidationAssert.Rejects(validator, command with { Units = [Phase2BCommands.BagUnit(_ruleR.Id, quantity: 0)] });
        ValidationAssert.Rejects(validator, command with { Context = null! });
        ValidationAssert.Rejects(validator, command with { Context = command.Context with { Travellers = [] } });
        ValidationAssert.Rejects(validator, command with { Context = command.Context with { Bounds = [] } });
    }

    [Fact]
    public async Task P2B_V12_ReadReturnsTheReservationAndAnUnknownIdIsNotFound()
    {
        var held = await _harness.ReserveAsync(Reserve("v12", Context(), Phase2BCommands.BagUnit(_ruleR.Id, "U1"), Phase2BCommands.LoungeUnit(_ruleRL.Id)));
        var read = await _harness.GetReservationAsync(held.ReservationId);

        Assert.Equal(Serialized(held), Serialized(read));

        const long unknown = 1;

        await BusinessAssert.ThrowsAsync(16401, 404, () => _harness.GetReservationAsync(unknown));
        await BusinessAssert.ThrowsAsync(16401, 404, () => _harness.ConfirmReservationAsync(unknown));
        await BusinessAssert.ThrowsAsync(16401, 404, () => _harness.ReleaseReservationAsync(unknown));
        await BusinessAssert.ThrowsAsync(16401, 404, () => _harness.CancelReservationUnitsAsync(unknown, held.Units[0].UnitRef));
    }

    [Fact]
    public async Task P2B_V13_ConfirmConfirmsEveryUnitAndRepeatingChangesNothing()
    {
        var held = await _harness.ReserveAsync(Reserve("v13", Context(), Phase2BCommands.BagUnit(_ruleR.Id, "U1"), Phase2BCommands.LoungeUnit(_ruleRL.Id)));

        var confirmed = await _harness.ConfirmReservationAsync(held.ReservationId);
        var afterFirst = await StoredRowAsync(held.ReservationId);
        var again = await _harness.ConfirmReservationAsync(held.ReservationId);

        Assert.All(confirmed.Units, unit => Assert.Equal(ServiceReservationUnitStatus.Confirmed, unit.Status));
        Assert.All(again.Units, unit => Assert.Equal(ServiceReservationUnitStatus.Confirmed, unit.Status));
        Assert.Equal(afterFirst, await StoredRowAsync(held.ReservationId));
        Assert.All((await _harness.GetReservationAsync(held.ReservationId)).Units, unit => Assert.Equal(ServiceReservationUnitStatus.Confirmed, unit.Status));
    }

    [Fact]
    public async Task P2B_V14_ReleaseReleasesEveryUnitAndConflictsAreRefused()
    {
        var held = await _harness.ReserveAsync(Reserve("v14-a", Context(), Phase2BCommands.BagUnit(_ruleR.Id, "U1"), Phase2BCommands.LoungeUnit(_ruleRL.Id)));

        var released = await _harness.ReleaseReservationAsync(held.ReservationId);
        var afterFirst = await StoredRowAsync(held.ReservationId);
        var again = await _harness.ReleaseReservationAsync(held.ReservationId);

        Assert.All(released.Units, unit => Assert.Equal(ServiceReservationUnitStatus.Released, unit.Status));
        Assert.All(again.Units, unit => Assert.Equal(ServiceReservationUnitStatus.Released, unit.Status));
        Assert.Equal(afterFirst, await StoredRowAsync(held.ReservationId));
        await BusinessAssert.ThrowsAsync(16407, 409, () => _harness.ConfirmReservationAsync(held.ReservationId));

        var confirmed = await _harness.ReserveAsync(ReserveX("v14-b"));

        await _harness.ConfirmReservationAsync(confirmed.ReservationId);
        await BusinessAssert.ThrowsAsync(16410, 409, () => _harness.ReleaseReservationAsync(confirmed.ReservationId));
        Assert.Equal(ServiceReservationUnitStatus.Confirmed, Assert.Single((await _harness.GetReservationAsync(confirmed.ReservationId)).Units).Status);
    }

    [Fact]
    public async Task P2B_V15_HeldUnitsCountAsExpiredFromExpiresAtWithoutAnyWrite()
    {
        var held = await _harness.ReserveAsync(ReserveX("v15-held"));
        var confirmed = await _harness.ReserveAsync(ReserveX("v15-confirmed"));

        await _harness.ConfirmReservationAsync(confirmed.ReservationId);

        var heldRow = await StoredRowAsync(held.ReservationId);
        var confirmedRow = await StoredRowAsync(confirmed.ReservationId);

        _harness.Clock.Now = held.ExpiresAt.AddTicks(-1);

        Assert.Equal(ServiceReservationUnitStatus.Held, Assert.Single((await _harness.GetReservationAsync(held.ReservationId)).Units).Status);

        _harness.Clock.Now = held.ExpiresAt;

        Assert.Equal(ServiceReservationUnitStatus.Expired, Assert.Single((await _harness.GetReservationAsync(held.ReservationId)).Units).Status);
        await BusinessAssert.ThrowsAsync(16406, 409, () => _harness.ConfirmReservationAsync(held.ReservationId));
        await BusinessAssert.ThrowsAsync(16406, 409, () => _harness.ReleaseReservationAsync(held.ReservationId));
        await BusinessAssert.ThrowsAsync(16406, 409, () => _harness.CancelReservationUnitsAsync(held.ReservationId, held.Units[0].UnitRef));
        Assert.Equal(ServiceReservationUnitStatus.Expired, Assert.Single((await _harness.ReserveAsync(ReserveX("v15-held") with { ExpiresAt = held.ExpiresAt })).Units).Status);

        _harness.Clock.Now = held.ExpiresAt.AddDays(1);

        Assert.Equal(ServiceReservationUnitStatus.Confirmed, Assert.Single((await _harness.GetReservationAsync(confirmed.ReservationId)).Units).Status);
        Assert.Equal(ServiceReservationUnitStatus.Confirmed, Assert.Single((await _harness.ConfirmReservationAsync(confirmed.ReservationId)).Units).Status);
        Assert.Equal((heldRow, confirmedRow), (await StoredRowAsync(held.ReservationId), await StoredRowAsync(confirmed.ReservationId)));
    }

    [Fact]
    public async Task P2B_V16_CancelCancelsConfirmedUnitsOnly()
    {
        var confirmed = await _harness.ReserveAsync(ReserveX("v16-confirmed"));
        var held = await _harness.ReserveAsync(ReserveX("v16-held"));
        var unitRef = confirmed.Units[0].UnitRef;

        await _harness.ConfirmReservationAsync(confirmed.ReservationId);

        var cancelled = await _harness.CancelReservationUnitsAsync(confirmed.ReservationId, unitRef);
        var afterFirst = await StoredRowAsync(confirmed.ReservationId);
        var again = await _harness.CancelReservationUnitsAsync(confirmed.ReservationId, unitRef);

        Assert.Equal(ServiceReservationUnitStatus.Cancelled, Assert.Single(cancelled.Units).Status);
        Assert.Equal(ServiceReservationUnitStatus.Cancelled, Assert.Single(again.Units).Status);
        Assert.Equal(afterFirst, await StoredRowAsync(confirmed.ReservationId));

        await BusinessAssert.ThrowsAsync(16411, 409, () => _harness.CancelReservationUnitsAsync(held.ReservationId, held.Units[0].UnitRef));
        await BusinessAssert.ThrowsAsync(16412, 422, () => _harness.CancelReservationUnitsAsync(held.ReservationId, unitRef));
        await BusinessAssert.ThrowsAsync(16412, 422, () => _harness.CancelReservationUnitsAsync(confirmed.ReservationId));
        Assert.Equal(ServiceReservationUnitStatus.Held, Assert.Single((await _harness.GetReservationAsync(held.ReservationId)).Units).Status);
    }

    [Fact]
    public async Task P2B_V17_CancellingOneOfTwoConfirmedUnitsLeavesTheOtherConfirmed()
    {
        var held = await _harness.ReserveAsync(Reserve("v17", Context(), Phase2BCommands.BagUnit(_ruleR.Id, "U1"), Phase2BCommands.LoungeUnit(_ruleRL.Id)));
        var bag = held.Units[0].UnitRef;
        var lounge = held.Units[1].UnitRef;

        await _harness.ConfirmReservationAsync(held.ReservationId);

        var partly = await _harness.CancelReservationUnitsAsync(held.ReservationId, bag);

        Assert.Equal([ServiceReservationUnitStatus.Cancelled, ServiceReservationUnitStatus.Confirmed], partly.Units.Select(unit => unit.Status));
        await BusinessAssert.ThrowsAsync(16408, 409, () => _harness.ConfirmReservationAsync(held.ReservationId));
        await BusinessAssert.ThrowsAsync(16409, 409, () => _harness.CancelReservationUnitsAsync(held.ReservationId, bag, lounge));
        Assert.Equal(
            [ServiceReservationUnitStatus.Cancelled, ServiceReservationUnitStatus.Confirmed],
            (await _harness.GetReservationAsync(held.ReservationId)).Units.Select(unit => unit.Status));

        var all = await _harness.CancelReservationUnitsAsync(held.ReservationId, lounge);

        Assert.All(all.Units, unit => Assert.Equal(ServiceReservationUnitStatus.Cancelled, unit.Status));
    }

    [Fact]
    public async Task P2B_V18_ConfirmIgnoresLaterChangesOfProductAndPrice()
    {
        var held = await _harness.ReserveAsync(ReserveX("v18"));
        var revised = await _harness.ReviseProductAsync(_productX.Id);

        await _harness.ActivateProductAsync(revised.Id);
        await _harness.RetireProductAsync(revised.Id);
        await _harness.RetirePriceRuleAsync(_ruleR.Id);

        var confirmed = await _harness.ConfirmReservationAsync(held.ReservationId);
        var unit = Assert.Single(confirmed.Units);

        Assert.Equal(
            (ServiceReservationUnitStatus.Confirmed, 1, _ruleR.Id, 38.50m, AncillaryInventoryControl.Unlimited),
            (unit.Status, unit.ProductVersion, unit.PriceRuleId, unit.Total, unit.InventoryControl));
        Assert.Equal(Serialized(confirmed), Serialized(await _harness.GetReservationAsync(held.ReservationId)));
    }

    [Fact]
    public async Task P2B_V20_ReservationOperationsWriteNoOutboxMessage()
    {
        var first = await _harness.ReserveAsync(Reserve("v20-a", Context(), Phase2BCommands.BagUnit(_ruleR.Id, "U1"), Phase2BCommands.LoungeUnit(_ruleRL.Id)));
        var second = await _harness.ReserveAsync(ReserveX("v20-b"));

        await _harness.GetReservationAsync(first.ReservationId);
        await _harness.ConfirmReservationAsync(first.ReservationId);
        await _harness.CancelReservationUnitsAsync(first.ReservationId, first.Units[0].UnitRef);
        await _harness.ReleaseReservationAsync(second.ReservationId);

        Assert.Equal(0, await _harness.RunAsync(scope => scope.Command.OutboxMessages.CountAsync()));
    }

    [Fact]
    public void P2B_V20_ReservationServicesDependOnNothingOutsideThisService()
    {
        Type[] allowed =
        [
            typeof(IServiceReservationRepository),
            typeof(IAncillaryProductRepository),
            typeof(IAncillaryPriceRuleRepository),
            typeof(IUnitOfWork),
            typeof(IIdGenerator),
            typeof(IClock)
        ];
        Type[] services =
        [
            typeof(ReserveServiceReservationService),
            typeof(ConfirmServiceReservationService),
            typeof(ReleaseServiceReservationService),
            typeof(CancelServiceReservationUnitsService),
            typeof(GetServiceReservationByIdService)
        ];

        foreach (var service in services)
        {
            var parameters = Assert.Single(service.GetConstructors(BindingFlags.Public | BindingFlags.Instance)).GetParameters();

            Assert.All(parameters, parameter => Assert.Contains(parameter.ParameterType, allowed));
        }
    }

    private string Key(string name) => KeyPrefix + name;

    private ReservationContextInput Context(params ReservationExistingOccurrenceInput[] existing)
        => Phase2BCommands.Context(Airline, _harness.Clock.Now, existing);

    private ServiceReserveServiceReservationCommand Reserve(string name, ReservationContextInput context, params ReservationUnitInput[] units)
        => Phase2BCommands.Reserve(Key(name), _harness.Clock.Now.AddMinutes(30), context, units);

    private ServiceReserveServiceReservationCommand ReserveX(string name)
        => Reserve(name, Context(), Phase2BCommands.BagUnit(_ruleR.Id));

    private static string Serialized(object value) => JsonSerializer.Serialize(value, ApiJson.Options);

    private Task<int> StoredReservationsAsync()
        => _harness.RunAsync(scope => scope.Command.ServiceReservations.CountAsync(reservation => reservation.IdempotencyKey.StartsWith(KeyPrefix)));

    private Task<int> StoredUnitsAsync()
        => _harness.RunAsync(scope => scope.Command.ServiceReservations
            .Where(reservation => reservation.IdempotencyKey.StartsWith(KeyPrefix))
            .SelectMany(reservation => reservation.Units)
            .CountAsync());

    private Task<string> StoredRowAsync(long reservationId)
        => _harness.RunAsync(async scope =>
        {
            var reservation = await scope.Command.ServiceReservations
                .AsNoTracking()
                .Include(row => row.Units)
                .SingleAsync(row => row.Id == reservationId);

            return string.Join(
                '|',
                reservation.Units
                    .OrderBy(unit => unit.Id)
                    .Select(unit => $"{unit.Id}:{unit.Status}:{unit.LastUpdateTime:O}")
                    .Prepend($"{reservation.Id}:{reservation.LastUpdateTime:O}:{Convert.ToHexString(reservation.RowVersion)}"));
        });
}
