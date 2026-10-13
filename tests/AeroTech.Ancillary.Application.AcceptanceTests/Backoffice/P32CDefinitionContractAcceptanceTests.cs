using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using DefinitionController = AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Controllers.BackofficeController;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Backoffice;

[Collection(DatabaseCollection.Name)]
public class P32CDefinitionContractAcceptanceTests
{
    private static readonly string[] AirportServiceFields =
    [
        "airportId", "terminalRef", "facilityId", "direction", "serviceWindowStart", "serviceWindowEnd", "ianaTimeZone", "visitDurationMinutes",
        "maxGuestsPerPrimary", "requiresSpecificAppointment"
    ];

    private static readonly Dictionary<string, string[]> Fields = new()
    {
        ["A01"] = ["maxKgPerPiece"],
        ["A02"] = ["packageWeightKg"],
        ["A03"] = ["allowanceConcept", "maxKgPerPiece", "weightFromExclusiveKg", "weightToInclusiveKg", "chargeCombination"],
        ["A04"] = ["allowanceConcept", "maxKgPerPiece", "maxSize", "maxLinearSumCm", "chargeCombination"],
        ["A05"] = ["allowanceConcept", "maxKgPerPiece", "maxSize"],
        ["A06"] = ["allowanceConcept", "maxKgPerPiece", "maxSize", "maxLinearSumCm", "equipmentKind"],
        ["A07"] = ["seatCharacteristicCodes", "applicableCabinIds", "requiresExitRowEligibility"],
        ["A08"] = ["seatCharacteristicCodes", "applicableCabinIds", "requiresExitRowEligibility"],
        ["A09"] =
        [
            "seatCharacteristicCodes", "applicableCabinIds", "requiresExitRowEligibility", "requiresAdjacentSeat", "extraSeatPurpose",
            "extraOccupiedSeatCount", "requiresExternalTicketAction"
        ],
        ["A10"] = ["fromCabinId", "toCabinId", "allowedUpgradeKind", "requiresTicketExchange", "eligibleFareFamilyIds"],
        ["A11"] = ["mealCode", "dietaryCode", "cateringLeadTimeMinutes", "exclusiveMealFamilyCode"],
        ["A12"] = ["mealCode", "menuItemRef", "dietaryCode", "cateringLeadTimeMinutes", "exclusiveMealFamilyCode"],
        ["A13"] = ["allowedAnimalTypes", "maxCombinedWeightKg", "carrierDimensionsMaxCm", "minAnimalAgeWeeks", "requiredDocumentCodes"],
        ["A14"] =
        [
            "allowedAnimalTypes", "maxCombinedWeightKg", "carrierDimensionsMaxCm", "minAnimalAgeWeeks", "requiredDocumentCodes",
            "allowedHoldAnimalSizeBrackets"
        ],
        ["A15"] = ["wheelchair"],
        ["A16"] = ["disabilityAssistance"],
        ["A17"] = ["medicalEquipment"],
        ["A18"] = ["bassinet"],
        ["A19"] = ["unaccompaniedMinor"],
        ["A20"] = AirportServiceFields,
        ["A21"] = AirportServiceFields,
        ["A22"] = [.. AirportServiceFields[..9], "includedComponentCodes", "requiresSpecificAppointment"],
        ["A23"] = ["kind", "priorityZoneCode", "priorityGroupCode", "fareBenefitRef", "airportIds"],
        ["A24"] = ["planKind", "durationMinutes", "includedDataMb", "maxDevices", "eligibleAircraftIds", "deliveryStage", "fulfillmentProviderRef"]
    };

    private static readonly Dictionary<string, string[]> DetailFields = new()
    {
        ["A15"] = ["allowedSsrCodes", "assistanceLevelCode", "leadTimeMinutes"],
        ["A16"] = ["allowedSsrCodes", "requiredCommunicationMethod"],
        ["A17"] = ["medicalServiceCode", "requiresMedicalApproval", "equipmentKind", "oxygenUnits", "evidenceTypeCodes"],
        ["A18"] = ["maxInfantWeightKg", "maxInfantAgeMonths", "compatibleSeatGroups"],
        ["A19"] = ["minAgeYears", "maxAgeYearsExclusive", "connectionPolicy", "allowedTransitAirportIds"]
    };

    private static readonly Dictionary<AncillaryProfile, ProfileCalls> Calls = new()
    {
        [AncillaryProfile.Baggage] = new(
            (controller, body) => controller.DefineBaggage(Define<BaggageSpecificationRequest>(body), CancellationToken.None),
            (controller, id, body) => controller.ChangeBaggage(id, Change<BaggageSpecificationRequest>(body), CancellationToken.None),
            (controller, id) => controller.GetBaggage(id, CancellationToken.None)),
        [AncillaryProfile.Seat] = new(
            (controller, body) => controller.DefineSeat(Define<SeatSpecificationRequest>(body), CancellationToken.None),
            (controller, id, body) => controller.ChangeSeat(id, Change<SeatSpecificationRequest>(body), CancellationToken.None),
            (controller, id) => controller.GetSeat(id, CancellationToken.None)),
        [AncillaryProfile.Upgrade] = new(
            (controller, body) => controller.DefineUpgrade(Define<UpgradeSpecificationInput>(body), CancellationToken.None),
            (controller, id, body) => controller.ChangeUpgrade(id, Change<UpgradeSpecificationInput>(body), CancellationToken.None),
            (controller, id) => controller.GetUpgrade(id, CancellationToken.None)),
        [AncillaryProfile.Meal] = new(
            (controller, body) => controller.DefineMeal(Define<MealSpecificationRequest>(body), CancellationToken.None),
            (controller, id, body) => controller.ChangeMeal(id, Change<MealSpecificationRequest>(body), CancellationToken.None),
            (controller, id) => controller.GetMeal(id, CancellationToken.None)),
        [AncillaryProfile.Pet] = new(
            (controller, body) => controller.DefinePet(Define<PetSpecificationRequest>(body), CancellationToken.None),
            (controller, id, body) => controller.ChangePet(id, Change<PetSpecificationRequest>(body), CancellationToken.None),
            (controller, id) => controller.GetPet(id, CancellationToken.None)),
        [AncillaryProfile.AssistedTravel] = new(
            (controller, body) => controller.DefineAssistedTravel(Define<AssistedTravelSpecificationRequest>(body), CancellationToken.None),
            (controller, id, body) => controller.ChangeAssistedTravel(id, Change<AssistedTravelSpecificationRequest>(body), CancellationToken.None),
            (controller, id) => controller.GetAssistedTravel(id, CancellationToken.None)),
        [AncillaryProfile.AirportService] = new(
            (controller, body) => controller.DefineAirportService(Define<AirportServiceSpecificationRequest>(body), CancellationToken.None),
            (controller, id, body) => controller.ChangeAirportService(id, Change<AirportServiceSpecificationRequest>(body), CancellationToken.None),
            (controller, id) => controller.GetAirportService(id, CancellationToken.None)),
        [AncillaryProfile.Priority] = new(
            (controller, body) => controller.DefinePriority(Define<PrioritySpecificationInput>(body), CancellationToken.None),
            (controller, id, body) => controller.ChangePriority(id, Change<PrioritySpecificationInput>(body), CancellationToken.None),
            (controller, id) => controller.GetPriority(id, CancellationToken.None)),
        [AncillaryProfile.Connectivity] = new(
            (controller, body) => controller.DefineConnectivity(Define<ConnectivitySpecificationInput>(body), CancellationToken.None),
            (controller, id, body) => controller.ChangeConnectivity(id, Change<ConnectivitySpecificationInput>(body), CancellationToken.None),
            (controller, id) => controller.GetConnectivity(id, CancellationToken.None))
    };

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public P32CDefinitionContractAcceptanceTests(TestDatabase database) => _database = database;

    private sealed record ProfileCalls(
        Func<DefinitionController, string, Task<IActionResult>> Define,
        Func<DefinitionController, long, string, Task<IActionResult>> Change,
        Func<DefinitionController, long, Task<IActionResult>> Read);

    public static TheoryData<string> Variants => [.. AncillaryVariant.All.Select(variant => variant.Code).Order(StringComparer.Ordinal)];

    public static TheoryData<string> ExampleVariants
        => [.. AncillaryVariant.All.Where(variant => variant.Profile != AncillaryProfile.Seat).Select(variant => variant.Code).Order(StringComparer.Ordinal)];

    public static TheoryData<string, string, string> AgreeingV1Members => new()
    {
        { "A01", "chargeKind", "\"ExtraPiece\"" },
        { "A01", "allowanceConcept", "\"Piece\"" },
        { "A02", "allowanceConcept", "\"Weight\"" },
        { "A05", "chargeKind", "\"ExtraPiece\"" },
        { "A12", "mealKind", "\"PaidPreorder\"" },
        { "A11", "menuItemRef", "null" },
        { "A14", "transportMode", "\"Hold\"" },
        { "A13", "allowedHoldAnimalSizeBrackets", "[]" },
        { "A18", "assistanceKind", "\"Bassinet\"" },
        { "A18", "bassinet.requiresInfantAndGuardian", "true" },
        { "A19", "unaccompaniedMinor.guardianContactRequired", "true" },
        { "A15", "bassinet", "null" },
        { "A22", "kind", "\"Cip\"" },
        { "A20", "includedComponentCodes", "[]" }
    };

    public static TheoryData<string, string, string> ContradictingV1Members => new()
    {
        { "A02", "chargeKind", "\"ExtraPiece\"" },
        { "A01", "allowanceConcept", "\"Weight\"" },
        { "A01", "packageWeightKg", "10" },
        { "A04", "equipmentKind", "\"BIKE\"" },
        { "A12", "mealKind", "\"SpecialRequest\"" },
        { "A11", "menuItemRef", "\"MENU_PASTA_01\"" },
        { "A13", "transportMode", "\"Hold\"" },
        { "A15", "assistanceKind", "\"Bassinet\"" },
        { "A18", "bassinet.requiresInfantAndGuardian", "false" },
        { "A19", "unaccompaniedMinor.guardianContactRequired", "false" },
        { "A21", "kind", "\"Lounge\"" },
        { "A20", "includedComponentCodes", "[\"PORTER\"]" }
    };

    private static DefineServiceDefinitionRequest<TSpecification> Define<TSpecification>(string body)
        where TSpecification : class
        => HostJson.Bind<DefineServiceDefinitionRequest<TSpecification>>(body);

    private static ChangeServiceDefinitionRequest<TSpecification> Change<TSpecification>(string body)
        where TSpecification : class
        => HostJson.Bind<ChangeServiceDefinitionRequest<TSpecification>>(body);

    private static string[] Members(JsonNode node) => node.AsObject().Select(member => member.Key).ToArray();

    private static long Id(JsonNode node) => long.Parse(node["id"]!.GetValue<string>());

    private static AncillaryProfile ProfileOf(string variantCode) => AncillaryVariant.Find(variantCode)!.Profile;

    private static string CreateBody(JsonNode read, int airlineId, long supplierId, Action<JsonObject>? specification = null)
    {
        var body = new JsonObject
        {
            ["ownerAirlineId"] = airlineId,
            ["serviceDefinitionRef"] = read["serviceDefinitionRef"]!.GetValue<string>()
        };

        foreach (var member in read["form"]!.AsObject())
            body[member.Key] = member.Value?.DeepClone();

        body["supplierId"] = supplierId.ToString();
        specification?.Invoke(body["specification"]!.AsObject());

        return body.ToJsonString();
    }

    private static void Set(JsonObject specification, string path, string value)
    {
        var names = path.Split('.');
        var target = names[..^1].Aggregate(specification, (node, name) => node[name]!.AsObject());

        target[names[^1]] = JsonNode.Parse(value);
    }

    private async Task<(int AirlineId, long SupplierId)> OwnerAsync()
    {
        var airlineId = _database.NextAirlineId();

        await using var desk = new ControllerDesk(_database, _clock);

        return (airlineId, (await desk.Scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId))).Id);
    }

    private async Task<JsonNode> CallAsync(Func<DefinitionController, Task<IActionResult>> call)
    {
        await using var desk = new ControllerDesk(_database, _clock);

        return HostJson.Data(await call(desk.ServiceDefinitions));
    }

    private Task<JsonNode> ReadAsync(string variantCode, long serviceDefinitionId)
        => CallAsync(controller => Calls[ProfileOf(variantCode)].Read(controller, serviceDefinitionId));

    private async Task<JsonNode> AuthoredWithTheV1SpecificationAsync(string variantCode)
    {
        var (airlineId, supplierId) = await OwnerAsync();
        long serviceDefinitionId;

        await using (var desk = new ControllerDesk(_database, _clock))
        {
            var definition = V122Catalog.Define(V122Catalog.Case(variantCode), airlineId, supplierId);

            serviceDefinitionId = (await desk.Scope.DefineServiceDefinition.DefineAsync(definition)).Id;
        }

        return await ReadAsync(variantCode, serviceDefinitionId);
    }

    private async Task<JsonNode> CreatedFromAsync(string variantCode, JsonNode read, Action<JsonObject>? specification = null)
    {
        var (airlineId, supplierId) = await OwnerAsync();
        var created = await CallAsync(controller => Calls[ProfileOf(variantCode)].Define(controller, CreateBody(read, airlineId, supplierId, specification)));

        return await ReadAsync(variantCode, Id(created));
    }

    [Fact]
    public async Task P32C_DEF_05_every_profile_route_refuses_a_definition_of_another_profile()
    {
        var baggage = Id(await AuthoredWithTheV1SpecificationAsync("A01"));
        var seat = Id(await AuthoredWithTheV1SpecificationAsync("A07"));

        foreach (var (profile, calls) in Calls)
        {
            var foreign = profile == AncillaryProfile.Baggage ? seat : baggage;

            await BusinessAssert.ThrowsAsync(16219, 409, () => CallAsync(controller => calls.Read(controller, foreign)));
        }
    }
}
