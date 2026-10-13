using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Backoffice;

[Collection(DatabaseCollection.Name)]
public class P32CSeatContractAcceptanceTests
{
    private const string StandardDocument = """{ "type": "EmdAssociated", "rfic": "A", "rfisc": "ST1" }""";
    private const string PreferredDocument = """{ "type": "EmdAssociated", "rfic": "A", "rfisc": "ST2" }""";
    private const string NoDocument = """{ "type": "None", "rfic": null, "rfisc": null }""";

    private static readonly string[] GenericReadMembers =
    [
        "id", "ownerAirlineId", "supplierId", "supplierName", "serviceDefinitionRef", "version", "serviceTypeCode", "serviceSubCode", "subCodeSource",
        "groupCode", "subGroupCode", "description1Code", "description2Code", "pricingUnit", "serviceDateBasis", "profile", "variantCode", "variantName",
        "documentRouting", "specification", "selectionContract", "commercialName", "description", "documentType", "documentRfic", "documentRfisc",
        "bookingMethod", "bookingSsrCode", "bookingSsimCode", "bookingConfirmationRequirement", "salesEffectiveFrom", "salesDiscontinueOn", "status",
        "createdAt", "activatedAt", "suspendedAt", "retiredAt"
    ];

    private static readonly string[] EditViewHeader =
    [
        "id", "ownerAirlineId", "serviceDefinitionRef", "version", "status", "profile", "variantName", "supplierName", "createdAt", "activatedAt",
        "suspendedAt", "retiredAt", "form"
    ];

    private static readonly Dictionary<string, SeatCase> Cases = new SeatCase[]
    {
        new(
            "A07", "SEAT_STANDARD", "ST1", "Standard seat", "Emd", StandardDocument, "RQST",
            """{ "seatCharacteristicCodes": ["W"], "applicableCabinIds": [1], "requiresExitRowEligibility": false }""",
            """
            {
              "seatPurpose": "Standard", "seatCharacteristicCodes": ["W"], "applicableCabinIds": [1], "requiresExitRowEligibility": false,
              "requiresAdjacentSeat": false, "extraSeatPurpose": null, "extraOccupiedSeatCount": null, "requiresExternalTicketAction": false
            }
            """,
            ["seatCharacteristicCodes", "applicableCabinIds", "requiresExitRowEligibility"]),
        new(
            "A08", "SEAT_PREFERRED", "ST2", "Extra legroom seat", "Emd", PreferredDocument, "RQST",
            """{ "seatCharacteristicCodes": ["E", "L"], "applicableCabinIds": [1], "requiresExitRowEligibility": true }""",
            """
            {
              "seatPurpose": "Preferred", "seatCharacteristicCodes": ["E", "L"], "applicableCabinIds": [1], "requiresExitRowEligibility": true,
              "requiresAdjacentSeat": false, "extraSeatPurpose": null, "extraOccupiedSeatCount": null, "requiresExternalTicketAction": false
            }
            """,
            ["seatCharacteristicCodes", "applicableCabinIds", "requiresExitRowEligibility"]),
        new(
            "A09", "SEAT_EXTRA", "ST3", "Extra seat for comfort", "TicketOrExchange", NoDocument, "EXST",
            """
            {
              "seatCharacteristicCodes": [], "applicableCabinIds": [1], "requiresExitRowEligibility": false, "requiresAdjacentSeat": true,
              "extraSeatPurpose": "PassengerComfort", "extraOccupiedSeatCount": 1, "requiresExternalTicketAction": true
            }
            """,
            """
            {
              "seatPurpose": "ExtraSeat", "seatCharacteristicCodes": [], "applicableCabinIds": [1], "requiresExitRowEligibility": false,
              "requiresAdjacentSeat": true, "extraSeatPurpose": "PassengerComfort", "extraOccupiedSeatCount": 1, "requiresExternalTicketAction": true
            }
            """,
            [
                "seatCharacteristicCodes", "applicableCabinIds", "requiresExitRowEligibility", "requiresAdjacentSeat", "extraSeatPurpose",
                "extraOccupiedSeatCount", "requiresExternalTicketAction"
            ])
    }.ToDictionary(seat => seat.Variant);

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public P32CSeatContractAcceptanceTests(TestDatabase database) => _database = database;

    private sealed record SeatCase(
        string Variant,
        string Reference,
        string SubCode,
        string Name,
        string Routing,
        string Document,
        string SsrCode,
        string Specification,
        string V1Specification,
        string[] Members);

    public static TheoryData<string> Variants => ["A07", "A08", "A09"];

    public static TheoryData<string, string, string> RefusedSpecifications => new()
    {
        {
            "A07", "A07",
            """{ "seatCharacteristicCodes": [], "applicableCabinIds": [], "requiresExitRowEligibility": false, "extraSeatPurpose": "PassengerComfort" }"""
        },
        { "A07", "A07", """{ "seatCharacteristicCodes": [], "applicableCabinIds": [], "requiresExitRowEligibility": false, "extraOccupiedSeatCount": 1 }""" },
        { "A07", "A07", """{ "seatCharacteristicCodes": [], "applicableCabinIds": [], "requiresExitRowEligibility": false, "requiresAdjacentSeat": true }""" },
        {
            "A07", "A07",
            """{ "seatCharacteristicCodes": [], "applicableCabinIds": [], "requiresExitRowEligibility": false, "requiresExternalTicketAction": true }"""
        },
        { "A08", "A08", """{ "seatCharacteristicCodes": [], "applicableCabinIds": [1], "requiresExitRowEligibility": true }""" },
        { "A08", "A08", """{ "applicableCabinIds": [1], "requiresExitRowEligibility": true }""" },
        {
            "A08", "A08",
            """{ "seatPurpose": "Standard", "seatCharacteristicCodes": ["E"], "applicableCabinIds": [1], "requiresExitRowEligibility": true }"""
        },
        {
            "A09", "A09",
            """
            {
              "seatCharacteristicCodes": [], "applicableCabinIds": [1], "requiresExitRowEligibility": false, "requiresAdjacentSeat": true,
              "extraOccupiedSeatCount": 1, "requiresExternalTicketAction": true
            }
            """
        },
        {
            "A09", "A09",
            """
            {
              "seatCharacteristicCodes": [], "applicableCabinIds": [1], "requiresExitRowEligibility": false, "requiresAdjacentSeat": true,
              "extraSeatPurpose": "CabinBaggage", "extraOccupiedSeatCount": 0, "requiresExternalTicketAction": true
            }
            """
        },
        { "A07", "A99", """{ "seatCharacteristicCodes": [], "applicableCabinIds": [], "requiresExitRowEligibility": false }""" },
        { "A07", "A01", """{ "seatCharacteristicCodes": [], "applicableCabinIds": [], "requiresExitRowEligibility": false }""" }
    };

    public static TheoryData<string> ForeignMembers =>
    [
        """{ "seatPurpos": "Standard", "seatCharacteristicCodes": [], "applicableCabinIds": [], "requiresExitRowEligibility": false }""",
        """{ "seatCharacteristicCodes": [], "applicableCabinIds": [], "requiresExitRowEligibility": false, "price": 10 }""",
        """{ "seatCharacteristicCodes": [], "applicableCabinIds": [], "requiresExitRowEligibility": false, "pointOfSaleId": 1 }""",
        """{ "seatCharacteristicCodes": [], "applicableCabinIds": [], "requiresExitRowEligibility": false, "availableSeats": ["12A"] }"""
    ];

    private static string Body(SeatCase seat, string specification, int? airlineId, long supplierId, string? variantCode = null, string? name = null)
    {
        var identity = airlineId is null
            ? string.Empty
            : $"""
               "ownerAirlineId": {airlineId},
                 "serviceDefinitionRef": "{seat.Reference}",

               """;

        return $$"""
                 {
                   {{identity}}  "supplierId": "{{supplierId}}",
                   "serviceSubCode": "{{seat.SubCode}}",
                   "subCodeSource": "CarrierDefined",
                   "serviceTypeCode": "F",
                   "groupCode": "SA",
                   "subGroupCode": null,
                   "description1Code": null,
                   "description2Code": null,
                   "pricingUnit": "PerSeat",
                   "serviceDateBasis": "FlightDeparture",
                   "variantCode": "{{variantCode ?? seat.Variant}}",
                   "documentRouting": "{{seat.Routing}}",
                   "specification": {{JsonNode.Parse(specification)!.ToJsonString()}},
                   "commercialName": "{{name ?? seat.Name}}",
                   "description": null,
                   "document": {{seat.Document}},
                   "booking": { "method": "Ssr", "ssrCode": "{{seat.SsrCode}}", "ssimCode": null },
                   "salesEffectiveFrom": "2026-11-01",
                   "salesDiscontinueOn": null
                 }
                 """;
    }

    private static string[] Members(JsonNode node) => node.AsObject().Select(member => member.Key).ToArray();

    private static long Id(JsonNode node) => long.Parse(node["id"]!.GetValue<string>());

    private async Task<(int AirlineId, long SupplierId)> OwnerAsync()
    {
        var airlineId = _database.NextAirlineId();

        await using var desk = new ControllerDesk(_database, _clock);

        return (airlineId, (await desk.Scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId))).Id);
    }

    private async Task<JsonNode> CreateAsync(string body)
    {
        await using var desk = new ControllerDesk(_database, _clock);

        return HostJson.Data(
            await desk.ServiceDefinitions.DefineSeat(HostJson.Bind<DefineServiceDefinitionRequest<SeatSpecificationRequest>>(body), CancellationToken.None));
    }

    private async Task<JsonNode> ChangeAsync(long serviceDefinitionId, string body)
    {
        await using var desk = new ControllerDesk(_database, _clock);

        return HostJson.Data(
            await desk.ServiceDefinitions.ChangeSeat(
                serviceDefinitionId,
                HostJson.Bind<ChangeServiceDefinitionRequest<SeatSpecificationRequest>>(body),
                CancellationToken.None));
    }

    private async Task<JsonNode> ReadAsync(long serviceDefinitionId)
    {
        await using var desk = new ControllerDesk(_database, _clock);

        return HostJson.Data(await desk.ServiceDefinitions.GetSeat(serviceDefinitionId, CancellationToken.None));
    }

    private async Task<JsonNode> ReadGenericAsync(long serviceDefinitionId)
    {
        await using var desk = new ControllerDesk(_database, _clock);

        return HostJson.Data(await desk.ServiceDefinitions.GetById(serviceDefinitionId, CancellationToken.None));
    }

    private async Task AssertSavesBackUnchangedAsync(SeatCase seat, long serviceDefinitionId)
    {
        var read = await ReadAsync(serviceDefinitionId);
        var form = read["form"]!;

        Assert.Equal(EditViewHeader, Members(read));
        Assert.Equal(
            typeof(ChangeServiceDefinitionRequest<SeatSpecificationRequest>).GetProperties().Select(member => JsonNamingPolicy.CamelCase.ConvertName(member.Name)),
            Members(form));
        Assert.Equal(seat.Members, Members(form["specification"]!));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(seat.Specification), form["specification"]));
        Assert.Equal(seat.Variant, form["variantCode"]!.GetValue<string>());
        Assert.Equal("PerSeat", form["pricingUnit"]!.GetValue<string>());
        Assert.Equal("Draft", read["status"]!["name"]!.GetValue<string>());
        Assert.Equal("Seat", read["profile"]!["name"]!.GetValue<string>());
        Assert.Equal(seat.Reference, read["serviceDefinitionRef"]!.GetValue<string>());

        await ChangeAsync(serviceDefinitionId, form.ToJsonString());

        Assert.Equal(HostJson.Text(read), HostJson.Text(await ReadAsync(serviceDefinitionId)));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task P32C_SEAT_03_the_v1_body_with_every_seat_field_is_still_accepted_on_create_and_on_change(string variant)
    {
        var seat = Cases[variant];
        var (airlineId, supplierId) = await OwnerAsync();
        var serviceDefinitionId = Id(await CreateAsync(Body(seat, seat.V1Specification, airlineId, supplierId)));

        await ChangeAsync(serviceDefinitionId, Body(seat, seat.V1Specification, null, supplierId, name: "Renamed seat"));

        var read = await ReadAsync(serviceDefinitionId);

        Assert.Equal("Renamed seat", read["form"]!["commercialName"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(seat.Specification), read["form"]!["specification"]));
    }

    [Theory]
    [MemberData(nameof(RefusedSpecifications))]
    public async Task P32C_SEAT_04_a_field_that_its_variant_forbids_or_a_missing_required_one_is_refused_by_the_domain_rule(
        string caseVariant,
        string variantCode,
        string specification)
    {
        var seat = Cases[caseVariant];
        var (airlineId, supplierId) = await OwnerAsync();

        await BusinessAssert.ThrowsAsync(16202, 422, () => CreateAsync(Body(seat, specification, airlineId, supplierId, variantCode)));
    }

    [Theory]
    [MemberData(nameof(ForeignMembers))]
    public void P32C_SEAT_05_a_member_that_is_not_part_of_the_seat_definition_is_refused_at_binding(string specification)
    {
        var body = Body(Cases["A07"], specification, 1, 1);

        Assert.Throws<JsonException>(() => HostJson.Bind<DefineServiceDefinitionRequest<SeatSpecificationRequest>>(body));
        Assert.Throws<JsonException>(
            () => HostJson.Bind<DefineServiceDefinitionRequest<SeatSpecificationRequest>>(body.Replace("\"description\": null,", "\"description\": null, \"price\": 10,")));
    }

    [Fact]
    public async Task P32C_SEAT_06_the_seat_edit_view_answers_only_for_a_seat_definition()
    {
        var (airlineId, supplierId) = await OwnerAsync();
        long baggageId;

        await using (var desk = new ControllerDesk(_database, _clock))
            baggageId = (await desk.Scope.DefineServiceDefinition.DefineAsync(V122Catalog.Define(V122Catalog.Case("A01"), airlineId, supplierId))).Id;

        await BusinessAssert.ThrowsAsync(16219, 409, () => ReadAsync(baggageId));
        await BusinessAssert.ThrowsAsync(16201, 404, () => ReadAsync(long.MaxValue));
    }

    [Fact]
    public async Task P32C_SEAT_07_the_generic_read_by_id_keeps_its_v1_members()
    {
        var seat = Cases["A08"];
        var (airlineId, supplierId) = await OwnerAsync();
        var serviceDefinitionId = Id(await CreateAsync(Body(seat, seat.V1Specification, airlineId, supplierId)));

        var read = await ReadGenericAsync(serviceDefinitionId);

        Assert.Equal(GenericReadMembers, Members(read));
        Assert.Equal("Preferred", read["specification"]!["seat"]!["seatPurpose"]!["name"]!.GetValue<string>());
        Assert.Equal(9, read["specification"]!.AsObject().Count);
        Assert.Equal("EmdAssociated", read["documentType"]!["name"]!.GetValue<string>());
        Assert.Equal("Ssr", read["bookingMethod"]!["name"]!.GetValue<string>());
    }
}
