using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetProvisionPricingsPaginated.Backoffice;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetProvisionPricingsPaginated;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionForm;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionForm;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Controllers;
using AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Backoffice;

[Collection(DatabaseCollection.Name)]
public class SeatProvisionFacadeAcceptanceTests
{
    private const string Seat = "/Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId}";

    private static readonly string[] SeatMembers =
    [
        "sequence", "coverageScope", "purchaseStage", "priceOrigin", "quoteProviderKey", "quantity", "outcome", "settlement", "availability", "fulfillment",
        "passengerEligibility", "salesRestrictions", "geography", "flightApplication", "fareApplication", "travelDate", "dayTimeApplication", "advancePurchase",
        "seatApplication"
    ];

    private static readonly string[] ForeignMembers =
        ["serviceDefinitionId", "applicationType", "baggageApplication", "petRule", "assistedTravelRule", "airportServiceRule", "pricing", "remainingSeats"];

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public SeatProvisionFacadeAcceptanceTests(TestDatabase database) => _database = database;

    public static TheoryData<string, int?, bool> SeatVariants => new()
    {
        { "A07", 1, false },
        { "A08", 1, true },
        { "A09", null, false }
    };

    private static string[] Members(JsonNode node) => node.AsObject().Select(member => member.Key).ToArray();

    private static SeatProvisionRequest Request(TestDefineProvisionCommand rule)
        => new(
            rule.Sequence,
            rule.CoverageScope,
            rule.PurchaseStage,
            rule.PriceOrigin,
            rule.QuoteProviderKey,
            rule.Quantity,
            rule.Outcome,
            rule.Settlement,
            rule.Availability,
            rule.Fulfillment,
            rule.PassengerEligibility,
            rule.SalesRestrictions,
            rule.Geography,
            rule.FlightApplication,
            rule.FareApplication,
            rule.TravelDate,
            rule.DayTimeApplication,
            rule.AdvancePurchase,
            rule.SeatApplication);

    private static TestDefineProvisionCommand Free(TestDefineProvisionCommand rule)
        => rule with { Outcome = new ProvisionOutcomeInput(CommercialDisposition.Free, false, false), Origin = PriceOrigin.Free };

    private static TestDefineProvisionCommand Advanced(TestDefineProvisionCommand rule)
        => rule with
        {
            PassengerEligibility = new ProvisionPassengerEligibilityInput([PassengerTypeCode.ADT, PassengerTypeCode.CHD], [new ProvisionAgeBandInput(2, 12)]),
            Geography = new ProvisionGeographyInput(AllowedRoutePairs: [new ProvisionRoutePairInput(P1Commands.Thr, P1Commands.Ist, RoutePairDirection.BothDirections)]),
            FareApplication = new ProvisionFareApplicationInput(AllowedFareFamilyIds: [1, 2], AllowedCabinClassIds: [1]),
            TravelDate = new ProvisionTravelDateInput(
                [new ProvisionDatePeriodInput(new DateOnly(2027, 1, 1), new DateOnly(2027, 12, 31))],
                [new ProvisionDatePeriodInput(new DateOnly(2027, 3, 18), new DateOnly(2027, 3, 25))]),
            DayTimeApplication = new ProvisionDayTimeApplicationInput(
                [new ProvisionDayTimeWindowInput(31, new TimeOnly(6, 0), new TimeOnly(22, 0), DayTimeRestrictionEffect.Allow)]),
            AdvancePurchase = new ProvisionAdvancePurchaseInput(24, TimeUnit.Hours, false, 330)
        };

    private async Task<(long DefinitionId, TestDefineProvisionCommand Rule)> DefinitionAsync(string variantCode, bool activate = false)
    {
        var airlineId = _database.NextAirlineId();
        var variant = V122Catalog.Case(variantCode);

        await using var desk = new ControllerDesk(_database, _clock);

        var supplier = V122Catalog.NeedsExternalSupplier(variant) ? M1Commands.ExternalSupplier(airlineId, V122Catalog.QuoteProvider) : M1Commands.LocalSupplier(airlineId);
        var supplierId = (await desk.Scope.RegisterSupplier.RegisterAsync(supplier)).Id;
        var definitionId = (await desk.Scope.DefineServiceDefinition.DefineAsync(V122Catalog.Define(variant, airlineId, supplierId))).Id;

        if (activate)
            await desk.Scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definitionId));

        return (definitionId, V122Catalog.Rule(variant, definitionId));
    }

    private async Task<TResult> DeskAsync<TResult>(Func<ControllerDesk, Task<TResult>> request)
    {
        await using var desk = new ControllerDesk(_database, _clock);

        return await request(desk);
    }

    private async Task<long> DefineSeatAsync(long definitionId, SeatProvisionRequest request)
        => long.Parse((await DeskAsync(async desk => HostJson.Data(await desk.SeatProvisions.Define(definitionId, request, CancellationToken.None))))["id"]!.GetValue<string>());

    private Task<JsonNode> SeatFormAsync(long definitionId, long provisionId)
        => DeskAsync(async desk => HostJson.Data(await desk.SeatProvisions.GetForm(definitionId, provisionId, CancellationToken.None)));

    private Task<JsonNode> GenericFormAsync(long provisionId)
        => DeskAsync(async desk => HostJson.Data(await desk.Provisions.GetForm(provisionId, CancellationToken.None)));

    private Task<JsonNode> ChangeSeatAsync(long definitionId, long provisionId, string body)
        => DeskAsync(async desk => HostJson.Data(
            await desk.SeatProvisions.Change(definitionId, provisionId, HostJson.Bind<SeatProvisionRequest>(body), CancellationToken.None)));

    private Task RefusedAsync(int code, int httpStatus, Func<ControllerDesk, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, () => DeskAsync(async desk =>
        {
            await request(desk);

            return true;
        }));

    [Theory]
    [MemberData(nameof(SeatVariants))]
    public async Task SEAT_01_the_create_descriptor_states_the_variant_its_fixed_type_and_the_limits_without_any_default(string variantCode, int? maxQuantity, bool preferred)
    {
        var (definitionId, _) = await DefinitionAsync(variantCode);
        var descriptor = await DeskAsync(async desk => HostJson.Data(await desk.SeatProvisions.GetProvisionForm(definitionId, CancellationToken.None)));
        var constraints = descriptor["constraints"]!;

        Assert.Equal(
            new[] { "serviceDefinitionId", "variantCode", "profile", "definitionStatus", "pricingUnit", "serviceDateBasis", "applicationType", "allowedRuleSections", "constraints" },
            Members(descriptor));
        Assert.Equal(
            (definitionId.ToString(), variantCode, "Seat", "Draft", "PerSeat", "FlightDeparture", "Seat"),
            (descriptor["serviceDefinitionId"]!.GetValue<string>(), descriptor["variantCode"]!.GetValue<string>(), descriptor["profile"]!["name"]!.GetValue<string>(),
                descriptor["definitionStatus"]!["name"]!.GetValue<string>(), descriptor["pricingUnit"]!["name"]!.GetValue<string>(),
                descriptor["serviceDateBasis"]!["name"]!.GetValue<string>(), descriptor["applicationType"]!["name"]!.GetValue<string>()));
        Assert.Equal(
            new[] { "PassengerEligibility", "SalesRestrictions", "Geography", "TravelDate", "DayTimeApplication", "AdvancePurchase", "FlightApplication", "FareApplication", "SeatApplication" },
            descriptor["allowedRuleSections"]!.AsArray().Select(section => section!.GetValue<string>()));
        Assert.Equal(
            new[] { "quantityUnit", "maxQuantity", "requiresSeatNumberOrCharacteristic", "seatNumberFilterRequiresAircraft", "publishedPointOfSaleCount", "requiresPreferredCharacteristic" },
            Members(constraints));
        Assert.Equal(
            ("Each", maxQuantity, true, true, 1, preferred),
            (constraints["quantityUnit"]!["name"]!.GetValue<string>(), constraints["maxQuantity"]?.GetValue<int>(), constraints["requiresSeatNumberOrCharacteristic"]!.GetValue<bool>(),
                constraints["seatNumberFilterRequiresAircraft"]!.GetValue<bool>(), constraints["publishedPointOfSaleCount"]!.GetValue<int>(),
                constraints["requiresPreferredCharacteristic"]!.GetValue<bool>()));
    }

    [Theory]
    [MemberData(nameof(SeatVariants))]
    public async Task SEAT_02_a_seat_provision_is_defined_read_and_saved_back_and_nothing_stored_changes(string variantCode, int? maxQuantity, bool preferred)
    {
        var (definitionId, rule) = await DefinitionAsync(variantCode);
        var provisionId = await DefineSeatAsync(definitionId, Request(Advanced(rule)));
        var view = await SeatFormAsync(definitionId, provisionId);
        var form = view["form"]!;
        var generic = await GenericFormAsync(provisionId);

        Assert.Equal(
            new[]
            {
                "id", "serviceDefinitionId", "status", "profile", "variantCode", "editable", "allowedRuleSections", "constraints", "createdAt", "activatedAt", "suspendedAt",
                "retiredAt", "form"
            },
            Members(view));
        Assert.Equal(SeatMembers, Members(form));
        Assert.Equal(typeof(SeatProvisionRequest).GetProperties().Select(member => JsonNamingPolicy.CamelCase.ConvertName(member.Name)), Members(form));
        Assert.Equal(
            (provisionId.ToString(), definitionId.ToString(), "Draft", "Seat", variantCode, true, maxQuantity, preferred),
            (view["id"]!.GetValue<string>(), view["serviceDefinitionId"]!.GetValue<string>(), view["status"]!["name"]!.GetValue<string>(), view["profile"]!["name"]!.GetValue<string>(),
                view["variantCode"]!.GetValue<string>(), view["editable"]!.GetValue<bool>(), view["constraints"]!["maxQuantity"]?.GetValue<int>(),
                view["constraints"]!["requiresPreferredCharacteristic"]!.GetValue<bool>()));
        Assert.Equal("Seat", generic["form"]!["applicationType"]!.GetValue<string>());
        Assert.All(SeatMembers, member => Assert.Equal(generic["form"]![member]?.ToJsonString(), form[member]?.ToJsonString()));
        Assert.All(new[] { "baggageApplication", "petRule", "assistedTravelRule", "airportServiceRule" }, member => Assert.Null(generic["form"]![member]));
        Assert.Equal(
            ("2027-03-18", "2027-03-25", 31, 24, 330),
            (form["travelDate"]!["blackoutPeriods"]![0]!["startDate"]!.GetValue<string>(), form["travelDate"]!["blackoutPeriods"]![0]!["endDate"]!.GetValue<string>(),
                form["dayTimeApplication"]!["windows"]![0]!["daysOfWeekMask"]!.GetValue<int>(), form["advancePurchase"]!["minimumPeriod"]!.GetValue<int>(),
                form["advancePurchase"]!["maximumPeriod"]!.GetValue<int>()));

        await ChangeSeatAsync(definitionId, provisionId, form.ToJsonString());

        Assert.Equal(HostJson.Text(view), HostJson.Text(await SeatFormAsync(definitionId, provisionId)));
        Assert.Equal(HostJson.Text(generic), HostJson.Text(await GenericFormAsync(provisionId)));

        var edited = JsonNode.Parse(form.ToJsonString())!;

        edited["sequence"] = 20;
        await ChangeSeatAsync(definitionId, provisionId, edited.ToJsonString());

        var saved = await SeatFormAsync(definitionId, provisionId);

        Assert.Equal(20, saved["form"]!["sequence"]!.GetValue<int>());
        Assert.Equal(form["travelDate"]!.ToJsonString(), saved["form"]!["travelDate"]!.ToJsonString());
        Assert.Equal(form["dayTimeApplication"]!.ToJsonString(), saved["form"]!["dayTimeApplication"]!.ToJsonString());
        Assert.Equal(form["seatApplication"]!.ToJsonString(), saved["form"]!["seatApplication"]!.ToJsonString());
    }

    [Fact]
    public async Task SEAT_03_a_member_outside_the_seat_contract_is_refused_and_never_ignored()
    {
        var (definitionId, rule) = await DefinitionAsync("A07");
        var provisionId = await DefineSeatAsync(definitionId, Request(rule));
        var form = (await SeatFormAsync(definitionId, provisionId))["form"]!;

        Assert.All(ForeignMembers, member =>
        {
            var body = JsonNode.Parse(form.ToJsonString())!;

            body[member] = null;
            Assert.Throws<JsonException>(() => HostJson.Bind<SeatProvisionRequest>(body.ToJsonString()));
        });
        Assert.NotNull(HostJson.Bind<SeatProvisionRequest>(form.ToJsonString()));
        Assert.All(ForeignMembers, member => Assert.Null(typeof(SeatProvisionRequest).GetProperty(member, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)));
    }

    [Fact]
    public async Task SEAT_04_a_definition_of_another_profile_or_a_provision_of_another_definition_is_refused_and_nothing_changes()
    {
        var (seatId, seatRule) = await DefinitionAsync("A07");
        var (otherSeatId, _) = await DefinitionAsync("A08");
        var (petId, petRule) = await DefinitionAsync("A13");
        var provisionId = await DefineSeatAsync(seatId, Request(seatRule));
        var petProvisionId = await DeskAsync(async desk => (await desk.Scope.DefineProvision.DefineAsync(petRule)).Id);
        var before = HostJson.Text(await GenericFormAsync(provisionId));
        var body = (await SeatFormAsync(seatId, provisionId))["form"]!.ToJsonString();

        await RefusedAsync(16201, 404, desk => desk.SeatProvisions.GetProvisionForm(987_654_321, CancellationToken.None));
        await RefusedAsync(16219, 409, desk => desk.SeatProvisions.GetProvisionForm(petId, CancellationToken.None));
        await RefusedAsync(16219, 409, desk => desk.SeatProvisions.Define(petId, Request(seatRule), CancellationToken.None));
        await RefusedAsync(16301, 404, desk => desk.SeatProvisions.GetForm(otherSeatId, provisionId, CancellationToken.None));
        await RefusedAsync(16301, 404, desk => desk.SeatProvisions.GetForm(seatId, 987_654_321, CancellationToken.None));
        await RefusedAsync(16219, 409, desk => desk.SeatProvisions.GetForm(petId, petProvisionId, CancellationToken.None));
        await RefusedAsync(16301, 404, desk => desk.SeatProvisions.Change(otherSeatId, provisionId, HostJson.Bind<SeatProvisionRequest>(body), CancellationToken.None));
        await RefusedAsync(16219, 409, desk => desk.SeatProvisions.Change(petId, petProvisionId, HostJson.Bind<SeatProvisionRequest>(body), CancellationToken.None));

        Assert.Equal(before, HostJson.Text(await GenericFormAsync(provisionId)));
        Assert.Equal(
            new[] { "1|0" },
            await DeskAsync(async desk => await desk.Scope.Command.Database
                .SqlQuery<string>($"SELECT CONCAT((SELECT COUNT(*) FROM Ancillary.AncillaryProvisions WHERE ServiceDefinitionId = {seatId}), '|', (SELECT COUNT(*) FROM Ancillary.AncillaryProvisions WHERE ServiceDefinitionId = {otherSeatId})) AS Value")
                .ToListAsync()));
    }

    [Fact]
    public async Task SEAT_05_a_stored_rule_of_another_profile_is_reported_and_never_dropped_from_the_form()
    {
        var (definitionId, rule) = await DefinitionAsync("A07");
        var provisionId = await DefineSeatAsync(definitionId, Request(Advanced(rule)));
        var stored = await DeskAsync(desk => desk.Scope.GetProvisionForm.ExecuteAsync(provisionId));
        var polluted = stored with { Form = stored.Form with { PetRule = new ProvisionPetRuleInput(null, null, 5m, ConfirmationRequirement.SubjectToConfirmation) } };

        var refused = await BusinessAssert.ThrowsAsync(16321, 409, () => new GetSeatProvisionFormService(new FixedForm(polluted)).ExecuteAsync(definitionId, provisionId));
        var kept = await new GetSeatProvisionFormService(new FixedForm(stored)).ExecuteAsync(definitionId, provisionId);

        Assert.Contains("PetRule", refused.Message, StringComparison.Ordinal);
        Assert.Equal((stored.Form.TravelDate, stored.Form.DayTimeApplication, stored.Form.Geography), (kept.Form.TravelDate, kept.Form.DayTimeApplication, kept.Form.Geography));
        Assert.Single(kept.Form.TravelDate!.BlackoutPeriods!);
    }

    [Fact]
    public async Task SEAT_06_the_seat_rules_of_the_domain_still_decide_and_the_limits_of_the_descriptor_are_the_real_ones()
    {
        var (standardId, standard) = await DefinitionAsync("A07", activate: true);
        var (extraId, extra) = await DefinitionAsync("A09", activate: true);
        var two = new ProvisionQuantityInput(AncillaryQuantityUnit.Each, 1, 2);

        await RefusedAsync(16302, 422, desk => desk.SeatProvisions.Define(standardId, Request(standard with { SeatApplication = null }), CancellationToken.None));
        await RefusedAsync(16302, 422, desk => desk.SeatProvisions.Define(
            standardId,
            Request(standard with { SeatApplication = new ProvisionSeatApplicationInput(["12A"], null), FlightApplication = null }),
            CancellationToken.None));

        var capped = await DefineSeatAsync(standardId, Request(Free(standard) with { Quantity = two }));
        var uncapped = await DefineSeatAsync(extraId, Request(Free(extra) with { Quantity = two, Sequence = 20 }));

        await RefusedAsync(16321, 409, desk => desk.Scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(capped)));
        await DeskAsync(desk => desk.Scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(uncapped)));

        var active = await SeatFormAsync(extraId, uncapped);

        Assert.Equal(("Active", false), (active["status"]!["name"]!.GetValue<string>(), active["editable"]!.GetValue<bool>()));
        await RefusedAsync(16303, 409, desk => desk.SeatProvisions.Change(
            extraId,
            uncapped,
            HostJson.Bind<SeatProvisionRequest>(active["form"]!.ToJsonString()),
            CancellationToken.None));
    }

    [Fact]
    public async Task SEAT_07_a_paid_seat_provision_is_published_with_its_price_through_the_existing_pricing_and_publish_use_cases()
    {
        var (definitionId, rule) = await DefinitionAsync("A08", activate: true);
        var provisionId = await DefineSeatAsync(definitionId, Request(rule));
        var otherProvisionId = await DefineSeatAsync(definitionId, Request(rule with { Sequence = 20 }));
        var pricingId = await DeskAsync(async desk => (await desk.Scope.DefinePricing.DefineAsync(Pricing(provisionId, P1Commands.Eur, Base(35m, name: "Preferred seat"), Tax("VAT", 3m, name: "Value added tax")))).Id);

        await RefusedAsync(16310, 409, desk => desk.Scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provisionId)));
        await Assert.ThrowsAnyAsync<Exception>(() => DeskAsync(desk => desk.Scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(otherProvisionId, pricingId))));
        await DeskAsync(desk => desk.Scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, pricingId)));

        var published = await SeatFormAsync(definitionId, provisionId);
        var listed = await DeskAsync(desk => desk.Scope.GetPricingById.ExecuteAsync(pricingId));

        Assert.Equal(("Active", false, "Filed"), (published["status"]!["name"]!.GetValue<string>(), published["editable"]!.GetValue<bool>(), published["form"]!["priceOrigin"]!.GetValue<string>()));
        Assert.Equal(("Active", provisionId), (listed.Status.Name, listed.AncillaryProvisionId));
        Assert.Equal("Draft", (await SeatFormAsync(definitionId, otherProvisionId))["status"]!["name"]!.GetValue<string>());

        var pricings = await DeskAsync(desk => new GetProvisionPricingsPaginatedService(desk.Scope.Query).ExecuteAsync(
            new BackofficeGetProvisionPricingsPaginatedQuery { AncillaryProvisionId = provisionId }));
        var row = Assert.Single(pricings.Results);
        var rate = Assert.Single(row.Rates);

        Assert.Equal((pricingId.ToString(), provisionId.ToString(), 1, "Active", "EUR"), (row.Id, row.AncillaryProvisionId, row.Version, row.Status.Name, Assert.Single(row.Currencies)));
        var component = Assert.Single(rate.Components);

        Assert.Equal((35m, "EUR"), (rate.BasePrice.Amount, rate.BasePrice.Currency));
        Assert.Equal(("Tax", "VAT", 3m, "EUR"), (component.Category.Name, component.Code, component.Amount.Amount, component.Amount.Currency));
        Assert.Empty((await DeskAsync(desk => new GetProvisionPricingsPaginatedService(desk.Scope.Query).ExecuteAsync(
            new BackofficeGetProvisionPricingsPaginatedQuery { AncillaryProvisionId = otherProvisionId }))).Results);
        await RefusedAsync(16301, 404, desk => new GetProvisionPricingsPaginatedService(desk.Scope.Query).ExecuteAsync(
            new BackofficeGetProvisionPricingsPaginatedQuery { AncillaryProvisionId = 987_654_321 }));
    }

    [Fact]
    public async Task SEAT_08_the_four_routes_are_published_for_the_backoffice_surface_with_the_seat_only_body()
    {
        var controller = typeof(SeatBackofficeController);
        var document = await PublishedOpenApi.DocumentAsync();
        var body = await PublishedOpenApi.RequestSchemaAsync("post", $"{Seat}/Provisions");
        var edit = await PublishedOpenApi.RequestSchemaAsync("put", $"{Seat}/Provisions/{{provisionId}}");

        Assert.Equal(SurfaceAuthorization.Backoffice, controller.GetCustomAttribute<AuthorizeAttribute>()!.Policy);
        Assert.Equal("AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Controllers", controller.Namespace);
        Assert.Equal(
            new[] { $"{Seat}/ProvisionForm", $"{Seat}/Provisions", $"{Seat}/Provisions/{{provisionId}}", $"{Seat}/Provisions/{{provisionId}}/Form" },
            document.Paths.Keys.Where(path => path.StartsWith(Seat + "/", StringComparison.Ordinal)).Order(StringComparer.Ordinal));
        Assert.Equal(SeatMembers.Order(StringComparer.Ordinal), body.ActualSchema.ActualProperties.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(SeatMembers.Order(StringComparer.Ordinal), edit.ActualSchema.ActualProperties.Keys.Order(StringComparer.Ordinal));
        Assert.All(ForeignMembers, member => Assert.DoesNotContain(member, body.ActualSchema.ActualProperties.Keys));
    }

    private sealed class FixedForm(BackofficeProvisionFormDto form) : IGetAncillaryProvisionFormService
    {
        public Task<BackofficeProvisionFormDto> ExecuteAsync(long provisionId, CancellationToken cancellationToken = default) => Task.FromResult(form);
    }
}
