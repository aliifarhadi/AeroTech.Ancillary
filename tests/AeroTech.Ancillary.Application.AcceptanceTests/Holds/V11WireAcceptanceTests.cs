using System.Text.Json;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices.Service;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto;
using AeroTech.Ancillary.RestApi.V1.AncillaryReservationAggregate.Requests;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Holds;

[Collection(DatabaseCollection.Name)]
public class V11WireAcceptanceTests
{
    private const string Body = """
        {"idempotencyKey":"O-12001-H1","orderId":12001,"reference":"ORD-12001","requestedExpiresAt":"2026-10-27T10:30:00Z",
         "baggages":[{"variantCode":"A01","products":[{"provisionId":701,"units":[
           {"orderServiceId":501,"unit":{"travellerId":1001},"coverage":{"flightIds":[801,802]},"selected":{"quantity":2}}]}]}],
         "meals":[{"variantCode":"A11","products":[{"provisionId":702,"units":[
           {"orderServiceId":502,"unit":{"travellerId":1002},"coverage":{"flightIds":[801]}}]}]}],
         "assistedTravel":[{"variantCode":"A15","products":[{"provisionId":703,"units":[
           {"orderServiceId":503,"unit":{"travellerId":1001},"coverage":{"flightIds":[801]},"selected":{"options":{"assistedTravel":{"assistanceSsrCode":"WCHR"}}}}]}]}],
         "airportServices":[{"variantCode":"A20","products":[{"provisionId":704,"units":[
           {"orderServiceId":504,"unit":{"travellerId":1001},"selected":{"options":{"airportService":{"timeWithOffset":"2026-10-27T08:00:00+03:30","guestCount":1}}}}]}]}]}
        """;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public V11WireAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private static long NextOrderId => Interlocked.Increment(ref _lastOrderId);

    private static long _lastOrderId = 1_110_000;

    private sealed record Lounge(long DefinitionId, long SingleProvisionId, long RangeProvisionId);

    private async Task<Lounge> LoungeAsync()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var definition = await _harness.Proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, supplierId, "V11_LOUNGE"));
        var single = await _harness.Proof.RuleAsync(M1Commands.LoungeProvision(definition.Id));
        var range = await _harness.Proof.RuleAsync(M1Commands.LoungeProvision(definition.Id, sequence: 200, maxQuantity: 2));

        await _harness.ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "V11_LOUNGE", definition.Id, InventoryAuthority.Unlimited));

        return new Lounge(definition.Id, single.Provision.Id, range.Provision.Id);
    }

    private TestHoldAncillaryServicesCommand Hold(long orderId, string key, params HoldSelection[] selections)
        => M1Commands.Hold($"V11-{orderId}-{key}", orderId, _clock.Now.AddMinutes(30), selections);

    private static ServiceHoldAncillaryServicesCommand Command(HoldAncillaryServicesRequest request)
        => new(
            request.IdempotencyKey,
            request.OrderId,
            request.Reference,
            request.RequestedExpiresAt,
            request.Baggages,
            request.Meals,
            request.AssistedTravel,
            request.AirportServices,
            request.PriorityServices,
            request.Connectivity,
            request.Upgrades);

    private static bool IsValid(string body)
        => new ServiceHoldAncillaryServicesCommandValidator().Validate(Command(HostJson.Bind<HoldAncillaryServicesRequest>(body))).IsValid;

    private static AncillaryUnitHoldDto HeldUnit(long unitId, long orderServiceId)
        => new(unitId, orderServiceId, AncillaryReservationUnitStatus.Held, null, new HoldUnitDto(1001), new HoldCoverageDto([801]), new HeldSelectedUnitDto(1));

    private static SeatUnitHoldDto HeldSeat(long unitId, long orderServiceId, string seatNumber, string seatMapSelectionId)
        => new(unitId, orderServiceId, AncillaryReservationUnitStatus.Held, null, new HoldUnitDto(1001), new HoldCoverageDto([801]), new HeldSelectedUnitDto(1), seatNumber, seatMapSelectionId);

    [Fact]
    public void V11_W01_the_pack_sample_binds_into_four_families_and_passes_the_shape_rules()
    {
        var request = HostJson.Bind<HoldAncillaryServicesRequest>(Body);
        var baggage = request.Baggages![0];
        var lounge = request.AirportServices![0].Products[0].Units[0];

        Assert.True(IsValid(Body));
        Assert.Equal(("A01", 701L, 2), (baggage.VariantCode, baggage.Products[0].ProvisionId, baggage.Products[0].Units[0].Selected!.Quantity!.Value));
        Assert.Equal(new long[] { 801, 802 }, baggage.Products[0].Units[0].Coverage!.FlightIds);
        Assert.Null(request.Meals![0].Products[0].Units[0].Selected);
        Assert.Equal("WCHR", request.AssistedTravel![0].Products[0].Units[0].Selected!.Options!.AssistedTravel!.AssistanceSsrCode);
        Assert.Null(lounge.Coverage);
        Assert.Equal(TimeSpan.FromHours(3.5), lounge.Selected!.Options!.AirportService!.TimeWithOffset!.Value.Offset);
    }

    [Theory]
    [InlineData("W02", "\"orderId\":12001,", "\"orderId\":12001,\"services\":[],")]
    [InlineData("W03", "\"orderServiceId\":501,", "\"orderServiceId\":501,\"coverageScope\":\"Sector\",")]
    [InlineData("W04", "\"provisionId\":701,", "\"provisionId\":701,\"serviceDefinitionId\":2,")]
    [InlineData("W05", "\"orderServiceId\":504,", "\"orderServiceId\":504,\"inventoryFacts\":{\"guestCount\":1},")]
    [InlineData("W06", "{\"provisionId\":702,", "{\"productKey\":\"1:702\",\"provisionId\":702,")]
    [InlineData("W10-baggage-takes-only-baggage", "\"selected\":{\"quantity\":2}", "\"selected\":{\"options\":{\"assistedTravel\":{\"assistanceSsrCode\":\"WCHR\"}}}")]
    [InlineData("W10-meals-take-no-options", "\"unit\":{\"travellerId\":1002},\"coverage\":{\"flightIds\":[801]}", "\"unit\":{\"travellerId\":1002},\"coverage\":{\"flightIds\":[801]},\"selected\":{\"options\":{\"baggage\":{\"bagRef\":\"B1\"}}}")]
    [InlineData("W10-one-key-only", "{\"assistanceSsrCode\":\"WCHR\"}", "{\"assistanceSsrCode\":\"WCHR\"},\"priority\":{\"optIn\":true}")]
    [InlineData("W11", "\"assistanceSsrCode\":\"WCHR\"", "\"assistanceSsrCode\":\"WCHR\",\"note\":\"x\"")]
    [InlineData("W19", "\"unit\":{\"travellerId\":1002}", "\"unit\":{\"travellerRef\":\"T2\"}")]
    [InlineData("W20", "\"coverage\":{\"flightIds\":[801,802]}", "\"coverage\":{\"flightRefs\":[\"F1\",\"F2\"]}")]
    [InlineData("W21", "\"orderId\":12001,", "\"orderId\":12001,\"coveredFlightIds\":[801],")]
    [InlineData("W28-pets", "\"orderId\":12001,", "\"orderId\":12001,\"pets\":[],")]
    [InlineData("W28-infantRef", "\"assistanceSsrCode\":\"WCHR\"", "\"assistanceSsrCode\":\"WCHR\",\"infantRef\":\"T2\"")]
    [InlineData("W28-guardianRef", "\"assistanceSsrCode\":\"WCHR\"", "\"assistanceSsrCode\":\"WCHR\",\"guardianRef\":\"T1\"")]
    [InlineData("A09", "\"orderId\":12001,", "\"orderId\":12001,\"extraSeats\":[],")]
    [InlineData("TravelInsurance", "\"orderId\":12001,", "\"orderId\":12001,\"travelInsurance\":[],")]
    public void V11_W02_W06_W10_W11_W19_W21_W28_a_member_outside_the_contract_is_refused_by_strict_json(string id, string anchor, string replacement)
    {
        Assert.True(Body.Contains(anchor, StringComparison.Ordinal), id);
        Assert.Throws<JsonException>(() => HostJson.Bind<HoldAncillaryServicesRequest>(Body.Replace(anchor, replacement)));
    }

    [Fact]
    public void V11_W09_W15_W28_the_shape_rules_refuse_duplicates_an_empty_unit_and_empty_holds()
    {
        Assert.False(IsValid(Body.Replace("\"orderServiceId\":502,", "\"orderServiceId\":501,")));
        Assert.False(IsValid(Body.Replace("\"unit\":{\"travellerId\":1002}", "\"unit\":{}")));
        Assert.False(IsValid(Body.Replace("[801,802]", "[801,801]")));
        Assert.False(IsValid(Body.Replace("\"quantity\":2", "\"quantity\":0")));
        Assert.False(IsValid(Body.Replace("\"travellerId\":1002", "\"travellerId\":0")));
        Assert.False(IsValid("""{"idempotencyKey":"K","orderId":1,"reference":"R","requestedExpiresAt":"2026-10-27T10:30:00Z"}"""));
        Assert.False(IsValid("""{"idempotencyKey":"K","orderId":1,"reference":"R","requestedExpiresAt":"2026-10-27T10:30:00Z","baggages":[]}"""));
    }

    [Fact]
    public void V11_W16_W27_the_receipt_has_five_members_with_ids_as_strings_and_statuses_by_name()
    {
        var receipt = new AncillaryHoldReceipt(
            9001,
            "ORD-12001",
            AncillaryReservationStatus.Held,
            new DateTimeOffset(2026, 10, 27, 10, 30, 0, TimeSpan.Zero),
            [
                new AncillaryHoldUnitReceipt(9101, 501, AncillaryReservationUnitStatus.Held),
                new AncillaryHoldUnitReceipt(9102, 502, AncillaryReservationUnitStatus.Released)
            ]);

        Assert.Equal(
            """{"holdId":"9001","reference":"ORD-12001","status":"Held","expiresAt":"2026-10-27T10:30:00+00:00","units":[{"unitId":"9101","orderServiceId":"501","status":"Held"},{"unitId":"9102","orderServiceId":"502","status":"Released"}]}""",
            JsonSerializer.Serialize(receipt, HostJson.Options));

        var request = HostJson.Bind<HoldAncillaryServicesRequest>(Body
            .Replace("\"provisionId\":701", "\"provisionId\":\"701\"")
            .Replace("[801,802]", "[\"801\",\"802\"]"));

        Assert.Equal(701L, request.Baggages![0].Products[0].ProvisionId);
        Assert.Equal(new long[] { 801, 802 }, request.Baggages[0].Products[0].Units[0].Coverage!.FlightIds);
    }

    [Fact]
    public void V11_W25_W26_the_hold_projection_records_write_the_agreed_family_shape()
    {
        var seats = new AncillaryVariantHoldDto<SeatUnitHoldDto>("A07", [new AncillaryProductHoldDto<SeatUnitHoldDto>(705, "SEAT_STANDARD", [HeldSeat(9105, 505, "12A", "SMS-1")])]);
        var meals = new AncillaryVariantHoldDto<HeldMealDetailsDto, AncillaryUnitHoldDto>(
            "A11",
            [new AncillaryProductHoldDto<HeldMealDetailsDto, AncillaryUnitHoldDto>(702, "MEAL_VGML", new HeldMealDetailsDto("VGML", null), [HeldUnit(9102, 502)])]);
        var upgrade = new AncillaryProductHoldDto<HeldUpgradeDetailsDto, AncillaryUnitHoldDto>(706, "UPG_FIXED", null, [HeldUnit(9106, 506)]);
        var seatJson = JsonSerializer.Serialize(seats, HostJson.Options);

        Assert.Equal(
            """{"variantCode":"A07","products":[{"provisionId":"705","serviceDefinitionRef":"SEAT_STANDARD","units":[{"unitId":"9105","orderServiceId":"505","status":"Held","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"quantity":1},"seatNumber":"12A","seatMapSelectionId":"SMS-1"}]}]}""",
            seatJson);
        Assert.DoesNotContain("pricingId", seatJson, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            """{"variantCode":"A11","products":[{"provisionId":"702","serviceDefinitionRef":"MEAL_VGML","details":{"mealCode":"VGML"},"units":[{"unitId":"9102","orderServiceId":"502","status":"Held","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"quantity":1}}]}]}""",
            JsonSerializer.Serialize(meals, HostJson.Options));
        Assert.DoesNotContain("details", JsonSerializer.Serialize(upgrade, HostJson.Options), StringComparison.Ordinal);
        Assert.Equal(
            """{"airportId":2,"facilityId":"9001"}""",
            JsonSerializer.Serialize(new HeldAirportServiceDetailsDto(2, 9001), HostJson.Options));
    }

    [Fact]
    public void V11_W29_the_hold_projection_types_selected_per_family_and_keeps_seat_fields_on_seat_units()
    {
        var baggage = new AncillaryUnitHoldDto<HeldBaggageOptionsDto>(
            9101,
            501,
            AncillaryReservationUnitStatus.Held,
            null,
            new HoldUnitDto(1001),
            new HoldCoverageDto([801]),
            new HeldSelectedUnitDto<HeldBaggageOptionsDto>(1, new HeldBaggageOptionsDto(new HeldBaggageSelectionDto("B-32", 27.5m, null, null))));
        var appointment = new HeldSelectedUnitDto<HeldAirportServiceOptionsDto>(
            1,
            new HeldAirportServiceOptionsDto(new HeldAirportServiceSelectionDto(new DateTimeOffset(2026, 10, 27, 8, 0, 0, TimeSpan.FromHours(3.5)), 1)));

        Assert.Equal(
            """{"unitId":"9101","orderServiceId":"501","status":"Held","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"quantity":1,"options":{"baggage":{"bagRef":"B-32","measuredWeightKg":27.5}}}}""",
            JsonSerializer.Serialize(baggage, HostJson.Options));
        Assert.Equal(
            """{"quantity":1,"options":{"airportService":{"timeWithOffset":"2026-10-27T08:00:00+03:30","guestCount":1}}}""",
            JsonSerializer.Serialize(appointment, HostJson.Options));
        Assert.Equal(
            """{"quantity":1,"options":{"assistedTravel":{"assistanceSsrCode":"WCHR"}}}""",
            JsonSerializer.Serialize(new HeldSelectedUnitDto<HeldAssistedTravelOptionsDto>(1, new HeldAssistedTravelOptionsDto(new HeldAssistedTravelSelectionDto("WCHR"))), HostJson.Options));
        Assert.All(
            new[] { typeof(AncillaryUnitHoldDto), typeof(AncillaryUnitHoldDto<>), typeof(HeldSelectedUnitDto), typeof(HeldSelectedUnitDto<>) },
            type => Assert.DoesNotContain(type.GetProperties(), property => property.Name.StartsWith("Seat", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task V11_W01_W12_a_family_hold_takes_the_definition_the_scope_and_a_fixed_quantity_from_the_provision()
    {
        var lounge = await LoungeAsync();
        var orderId = NextOrderId;
        await using var scope = new AncillaryScope(_database, _clock);

        var hold = await scope.HoldAncillaryServices.HoldAsync(Hold(orderId, "W01", M1Commands.Lounge(lounge.SingleProvisionId, orderId * 10 + 1, 71, [301])));
        var unit = Assert.Single((await scope.Reservations.GetAsync(hold.HoldId))!.Units);

        Assert.Equal((AncillaryReservationStatus.Held, orderId * 10 + 1), (hold.Status, Assert.Single(hold.Units).OrderServiceId));
        Assert.Equal(
            (lounge.DefinitionId, lounge.SingleProvisionId, ServiceCoverageScope.Sector, 1),
            (unit.ServiceDefinitionId, unit.ProvisionId, unit.CoverageScope, unit.Quantity));
    }

    [Fact]
    public async Task V11_W07_W08_W13_W14_a_variant_family_or_quantity_that_does_not_fit_the_provision_is_refused_before_any_write()
    {
        var lounge = await LoungeAsync();
        var orderId = NextOrderId;
        var single = M1Commands.Lounge(lounge.SingleProvisionId, orderId * 10 + 1, 71, [301]);
        await using var scope = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldAncillaryServices.HoldAsync(Hold(orderId, "W07", single with { VariantCode = AncillaryVariant.FastTrack })));
        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldAncillaryServices.HoldAsync(Hold(orderId, "W08", single with { Family = AncillaryProfile.Baggage })));
        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldAncillaryServices.HoldAsync(Hold(
            orderId, "W13", M1Commands.Lounge(lounge.RangeProvisionId, orderId * 10 + 1, 71, [301]))));
        await BusinessAssert.ThrowsAsync(16409, 422, () => scope.HoldAncillaryServices.HoldAsync(Hold(
            orderId, "W14", M1Commands.Lounge(lounge.RangeProvisionId, orderId * 10 + 1, 71, [301], quantity: 3))));

        Assert.Equal(
            new[] { "0" },
            await _harness.RowsAsync($"SELECT CONCAT(COUNT(*), '') AS Value FROM Ancillary.AncillaryReservations WHERE OrderId = {orderId}"));
    }

    [Fact]
    public async Task V11_D22_a_bassinet_is_refused_by_the_hold_even_without_a_confirmation_step()
    {
        var (_, airlineId, supplierId) = await _harness.OperatorAsync();
        var variant = V122Catalog.Case(AncillaryVariant.Bassinet);
        var definition = await _harness.Proof.DefinitionAsync(V122Catalog.Define(variant, airlineId, supplierId) with
        {
            Booking = variant.Booking with { ConfirmationRequirement = ConfirmationRequirement.Immediate }
        });
        var rule = await _harness.Proof.RuleAsync(V122Catalog.Rule(variant, definition.Id));
        var orderId = NextOrderId;
        await using var scope = new AncillaryScope(_database, _clock);

        var refused = await BusinessAssert.ThrowsAsync(16419, 409, () => scope.HoldAncillaryServices.HoldAsync(Hold(
            orderId,
            "D22",
            new HoldSelection(AncillaryProfile.AssistedTravel, variant.Code, rule.Provision.Id, orderId * 10 + 1, 71, [301], null, null))));

        Assert.Contains("variant A18", refused.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task V11_L01_a_replay_answers_from_the_stored_hold_after_the_provision_is_retired()
    {
        var lounge = await LoungeAsync();
        var orderId = NextOrderId;
        var command = Hold(orderId, "L01", M1Commands.Lounge(lounge.SingleProvisionId, orderId * 10 + 1, 71, [301]));
        AncillaryHoldReceipt first;

        await using (var scope = new AncillaryScope(_database, _clock))
            first = await scope.HoldAncillaryServices.HoldAsync(command);

        await using (var scope = new AncillaryScope(_database, _clock))
            await scope.RetireProvision.RetireAsync(new TestProvisionLifecycleCommand(lounge.SingleProvisionId));

        await using var replayScope = new AncillaryScope(_database, _clock);
        var replay = await replayScope.HoldAncillaryServices.HoldAsync(command);

        Assert.Equal((first.HoldId, first.Units.Single().UnitId), (replay.HoldId, replay.Units.Single().UnitId));
        await BusinessAssert.ThrowsAsync(16403, 409, () => replayScope.HoldAncillaryServices.HoldAsync(
            Hold(orderId, "L01", M1Commands.Lounge(lounge.SingleProvisionId, orderId * 10 + 1, 72, [301]))));
    }
}
