using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionForm;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm;
using AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests;
using AeroTech.Ancillary.Synchronizer.AncillaryProvisionAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Backoffice;

[Collection(DatabaseCollection.Name)]
public class ProfileProvisionFacadeAcceptanceTests
{
    private static readonly string[] ProfileSections = ["baggageApplication", "seatApplication", "petRule", "assistedTravelRule", "airportServiceRule"];

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public ProfileProvisionFacadeAcceptanceTests(TestDatabase database) => _database = database;

    public static TheoryData<string, string, string?> Profiles => new()
    {
        { "Baggage", "A01", "baggageApplication" },
        { "Upgrade", "A10", null },
        { "Meal", "A12", null },
        { "Pet", "A13", "petRule" },
        { "AssistedTravel", "A15", "assistedTravelRule" },
        { "AirportService", "A20", "airportServiceRule" },
        { "Priority", "A23", null },
        { "Connectivity", "A24", null }
    };

    private static string[] Members(JsonNode node) => node.AsObject().Select(member => member.Key).ToArray();

    private static object Controller(ControllerDesk desk, string profile) => typeof(ControllerDesk).GetProperty($"{profile}Provisions")!.GetValue(desk)!;

    private static Type RequestOf(string profile) => typeof(ControllerDesk).GetProperty($"{profile}Provisions")!.PropertyType.GetMethod("Define")!.GetParameters()[1].ParameterType;

    private static object Bind(string profile, string body) => JsonSerializer.Deserialize(body, RequestOf(profile), HostJson.Options) ?? throw new JsonException("The request body is empty.");

    private static async Task<IActionResult> CallAsync(ControllerDesk desk, string profile, string operation, params object[] arguments)
    {
        var controller = Controller(desk, profile);

        return await (Task<IActionResult>)controller.GetType().GetMethod(operation)!.Invoke(controller, [.. arguments, CancellationToken.None])!;
    }

    private static string Body(string profile, TestDefineProvisionCommand rule)
    {
        var source = JsonSerializer.SerializeToNode(rule, HostJson.Options)!.AsObject();
        var body = new JsonObject();

        foreach (var member in RequestOf(profile).GetProperties().Select(property => JsonNamingPolicy.CamelCase.ConvertName(property.Name)))
            body[member] = source[member]?.DeepClone();

        return body.ToJsonString();
    }

    private async Task<TResult> DeskAsync<TResult>(Func<ControllerDesk, Task<TResult>> request)
    {
        await using var desk = new ControllerDesk(_database, _clock);

        return await request(desk);
    }

    private Task RefusedAsync(int code, int httpStatus, Func<ControllerDesk, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, () => DeskAsync(async desk =>
        {
            await request(desk);

            return true;
        }));

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

    private Task<long> DefineAsync(string profile, long definitionId, string body)
        => DeskAsync(async desk => long.Parse(HostJson.Data(await CallAsync(desk, profile, "Define", definitionId, Bind(profile, body)))["id"]!.GetValue<string>()));

    private Task<JsonNode> FormAsync(string profile, long definitionId, long provisionId)
        => DeskAsync(async desk => HostJson.Data(await CallAsync(desk, profile, "GetForm", definitionId, provisionId)));

    private Task<JsonNode> ChangeAsync(string profile, long definitionId, long provisionId, string body)
        => DeskAsync(async desk => HostJson.Data(await CallAsync(desk, profile, "Change", definitionId, provisionId, Bind(profile, body))));

    private Task<JsonNode> GenericFormAsync(long provisionId)
        => DeskAsync(async desk => HostJson.Data(await desk.Provisions.GetForm(provisionId, CancellationToken.None)));

    private static JsonArray Rules(params JsonObject[] rules) => new(rules.Select(rule => (JsonNode)rule).ToArray());

    private static JsonObject Rule(string reference, string reason, int? from, int? to, string kind, decimal? amount = null, decimal? percentage = null, string? percentageBase = null)
        => new()
        {
            ["ruleRef"] = reference,
            ["reasonKind"] = reason,
            ["beforeStartMinutesFrom"] = from,
            ["beforeStartMinutesTo"] = to,
            ["penaltyKind"] = kind,
            ["penaltyFixedAmount"] = amount,
            ["penaltyPercentage"] = percentage,
            ["penaltyCurrencyId"] = amount is null ? null : P1Commands.Eur,
            ["percentageBase"] = percentageBase,
            ["waiverPermitted"] = false,
            ["sourceReference"] = kind is "ProviderQuoted" or "ManualReview" ? null : "CARRIER-FILED-TERMS"
        };

    private static string WithRules(string body, JsonNode? rules, bool omit = false)
    {
        var node = JsonNode.Parse(body)!;

        if (omit)
            node["settlement"]!.AsObject().Remove("cancellationRules");
        else
            node["settlement"]!["cancellationRules"] = rules?.DeepClone();

        return node.ToJsonString();
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public async Task PROF_01_the_create_descriptor_states_the_variant_its_type_its_sections_and_the_limits_the_domain_enforces(string profile, string variantCode, string? section)
    {
        var (definitionId, _) = await DefinitionAsync(variantCode);
        var descriptor = await DeskAsync(async desk => HostJson.Data(await CallAsync(desk, profile, "GetProvisionForm", definitionId)));
        var definition = await DeskAsync(async desk => (await desk.Scope.Definitions.GetAsync(definitionId))!);
        var constraints = descriptor["constraints"]!;

        static string[] Names(JsonNode node) => node.AsArray().Select(value => value!["name"]!.GetValue<string>()).ToArray();

        Assert.Equal(
            new[] { "serviceDefinitionId", "variantCode", "profile", "definitionStatus", "pricingUnit", "serviceDateBasis", "applicationType", "allowedRuleSections", "constraints" },
            Members(descriptor));
        Assert.Equal(
            (definitionId.ToString(), variantCode, profile, profile == "Baggage" ? "Baggage" : "Standard"),
            (descriptor["serviceDefinitionId"]!.GetValue<string>(), descriptor["variantCode"]!.GetValue<string>(), descriptor["profile"]!["name"]!.GetValue<string>(),
                descriptor["applicationType"]!["name"]!.GetValue<string>()));
        Assert.Equal(new[] { "quantityUnit", "maxQuantity", "allowedDispositions", "allowedPriceOrigins", "allowedPurchaseStages" }, Members(constraints));
        Assert.Equal(AncillaryProvision.QuantityUnitOf(definition).ToString(), constraints["quantityUnit"]!["name"]!.GetValue<string>());
        Assert.Null(constraints["maxQuantity"]);
        Assert.Equal(AncillaryProvision.AllowedDispositions(definition).Select(value => value.ToString()), Names(constraints["allowedDispositions"]!));
        Assert.Equal(AncillaryProvision.AllowedPriceOrigins(definition).Select(value => value.ToString()), Names(constraints["allowedPriceOrigins"]!));
        Assert.Equal(AncillaryProvision.AllowedPurchaseStages(definition).Select(value => value.ToString()), Names(constraints["allowedPurchaseStages"]!));

        var sections = descriptor["allowedRuleSections"]!.AsArray().Select(value => JsonNamingPolicy.CamelCase.ConvertName(value!.GetValue<string>())).ToList();

        Assert.All(ProfileSections, member => Assert.Equal(member == section, sections.Contains(member)));
        Assert.Equal(definition.ServiceDateBasis == ServiceDateBasis.FlightDeparture, sections.Contains("flightApplication") && sections.Contains("fareApplication"));
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public async Task PROF_02_a_typed_provision_is_defined_read_and_saved_back_unchanged_and_the_generic_form_agrees(string profile, string variantCode, string? section)
    {
        var (definitionId, rule) = await DefinitionAsync(variantCode);
        var provisionId = await DefineAsync(profile, definitionId, Body(profile, rule));
        var view = await FormAsync(profile, definitionId, provisionId);
        var form = view["form"]!;
        var generic = await GenericFormAsync(provisionId);
        var members = RequestOf(profile).GetProperties().Select(property => JsonNamingPolicy.CamelCase.ConvertName(property.Name)).ToArray();

        Assert.Equal(
            new[]
            {
                "id", "serviceDefinitionId", "status", "profile", "variantCode", "editable", "allowedRuleSections", "constraints", "createdAt", "activatedAt", "suspendedAt",
                "retiredAt", "form"
            },
            Members(view));
        Assert.Equal(members, Members(form));
        Assert.Equal(section is null ? 18 : 19, members.Length);
        Assert.Equal(
            (provisionId.ToString(), "Draft", profile, variantCode, true),
            (view["id"]!.GetValue<string>(), view["status"]!["name"]!.GetValue<string>(), view["profile"]!["name"]!.GetValue<string>(), view["variantCode"]!.GetValue<string>(),
                view["editable"]!.GetValue<bool>()));
        Assert.All(members, member => Assert.Equal(generic["form"]![member]?.ToJsonString(), form[member]?.ToJsonString()));
        Assert.All(ProfileSections.Where(member => member != section), member => Assert.Null(generic["form"]![member]));
        Assert.Equal(new[] { "reissueRefund", "formOfRefund", "commissionable", "interlineSettlement" }, Members(form["settlement"]!));

        if (section is not null)
            Assert.NotNull(form[section]);

        await ChangeAsync(profile, definitionId, provisionId, form.ToJsonString());

        Assert.Equal(HostJson.Text(view), HostJson.Text(await FormAsync(profile, definitionId, provisionId)));
        Assert.Equal(HostJson.Text(generic), HostJson.Text(await GenericFormAsync(provisionId)));

        var edited = JsonNode.Parse(form.ToJsonString())!;

        edited["sequence"] = 20;
        await ChangeAsync(profile, definitionId, provisionId, edited.ToJsonString());

        Assert.Equal(20, (await FormAsync(profile, definitionId, provisionId))["form"]!["sequence"]!.GetValue<int>());
    }

    [Fact]
    public async Task PROF_03_a_member_outside_the_shape_or_a_resource_of_another_profile_or_definition_is_refused()
    {
        var (mealId, mealRule) = await DefinitionAsync("A12");
        var (otherMealId, _) = await DefinitionAsync("A11");
        var (upgradeId, _) = await DefinitionAsync("A10");
        var (petId, petRule) = await DefinitionAsync("A13");
        var mealBody = Body("Meal", mealRule);
        var mealProvisionId = await DefineAsync("Meal", mealId, mealBody);
        var petProvisionId = await DefineAsync("Pet", petId, Body("Pet", petRule));
        var before = HostJson.Text(await GenericFormAsync(mealProvisionId));

        foreach (var (profile, foreign) in new[] { ("Meal", "petRule"), ("Meal", "seatApplication"), ("Pet", "baggageApplication"), ("Baggage", "airportServiceRule"), ("AirportService", "applicationType") })
        {
            var body = JsonNode.Parse(profile == "Pet" ? Body("Pet", petRule) : mealBody)!;

            body[foreign] = null;
            Assert.Throws<JsonException>(() => Bind(profile, body.ToJsonString()));
        }

        await RefusedAsync(16219, 409, desk => CallAsync(desk, "Upgrade", "GetProvisionForm", mealId));
        await RefusedAsync(16219, 409, desk => CallAsync(desk, "Upgrade", "Define", mealId, Bind("Upgrade", mealBody)));
        await RefusedAsync(16219, 409, desk => CallAsync(desk, "Meal", "Define", upgradeId, Bind("Meal", mealBody)));
        await RefusedAsync(16219, 409, desk => CallAsync(desk, "Meal", "GetForm", petId, petProvisionId));
        await RefusedAsync(16219, 409, desk => CallAsync(desk, "Upgrade", "Change", mealId, mealProvisionId, Bind("Upgrade", mealBody)));
        await RefusedAsync(16301, 404, desk => CallAsync(desk, "Meal", "GetForm", otherMealId, mealProvisionId));
        await RefusedAsync(16301, 404, desk => CallAsync(desk, "Meal", "Change", otherMealId, mealProvisionId, Bind("Meal", mealBody)));
        await RefusedAsync(16301, 404, desk => CallAsync(desk, "Meal", "GetForm", mealId, 987_654_321L));

        var stored = await DeskAsync(desk => desk.Scope.GetProvisionForm.ExecuteAsync(mealProvisionId));
        var polluted = stored with { Form = stored.Form with { SeatApplication = new ProvisionSeatApplicationInput(null, ["W"]) } };
        var refused = await BusinessAssert.ThrowsAsync(16321, 409, () => DeskAsync(desk =>
            new GetProfileProvisionFormService(new FixedForm(polluted), desk.Scope.Definitions).ExecuteAsync(AncillaryProfile.Meal, null, mealId, mealProvisionId)));

        Assert.Contains("SeatApplication", refused.Message, StringComparison.Ordinal);
        Assert.Equal(before, HostJson.Text(await GenericFormAsync(mealProvisionId)));
    }

    [Fact]
    public async Task PROF_04_cancellation_rules_ride_in_the_settlement_are_kept_when_left_out_cleared_by_an_empty_list_and_refused_when_they_overlap()
    {
        var (definitionId, rule) = await DefinitionAsync("A12");
        var filed = Rules(
            Rule("VOL-24H", "Voluntary", 1440, null, "FixedAmount", amount: 15m),
            Rule("VOL-LATE", "Voluntary", 0, 1440, "Percentage", percentage: 50m, percentageBase: "ServiceTotal"),
            Rule("SUPPLIER", "SupplierFailure", null, null, "ProviderQuoted"));
        var provisionId = await DefineAsync("Meal", definitionId, WithRules(Body("Meal", rule), filed));
        var form = (await FormAsync("Meal", definitionId, provisionId))["form"]!;
        var saved = form["settlement"]!["cancellationRules"]!;

        Assert.Equal(new[] { "VOL-24H", "VOL-LATE", "SUPPLIER" }, saved.AsArray().Select(value => value!["ruleRef"]!.GetValue<string>()));
        Assert.Equal(
            ("FixedAmount", 15m, P1Commands.Eur, "Percentage", 50m, "ServiceTotal"),
            (saved[0]!["penaltyKind"]!.GetValue<string>(), saved[0]!["penaltyFixedAmount"]!.GetValue<decimal>(), saved[0]!["penaltyCurrencyId"]!.GetValue<int>(),
                saved[1]!["penaltyKind"]!.GetValue<string>(), saved[1]!["penaltyPercentage"]!.GetValue<decimal>(), saved[1]!["percentageBase"]!.GetValue<string>()));
        Assert.Equal(saved.ToJsonString(), (await GenericFormAsync(provisionId))["form"]!["settlement"]!["cancellationRules"]!.ToJsonString());

        await ChangeAsync("Meal", definitionId, provisionId, WithRules(form.ToJsonString(), null, omit: true));
        Assert.Equal(saved.ToJsonString(), (await FormAsync("Meal", definitionId, provisionId))["form"]!["settlement"]!["cancellationRules"]!.ToJsonString());

        await ChangeAsync("Meal", definitionId, provisionId, WithRules(form.ToJsonString(), null));
        Assert.Equal(saved.ToJsonString(), (await FormAsync("Meal", definitionId, provisionId))["form"]!["settlement"]!["cancellationRules"]!.ToJsonString());

        await RefusedAsync(16302, 422, desk => CallAsync(desk, "Meal", "Change", definitionId, provisionId, Bind("Meal",
            WithRules(form.ToJsonString(), Rules(Rule("A", "Voluntary", 0, 2000, "ManualReview"), Rule("B", "Voluntary", 1440, null, "ManualReview"))))));
        Assert.Equal(saved.ToJsonString(), (await FormAsync("Meal", definitionId, provisionId))["form"]!["settlement"]!["cancellationRules"]!.ToJsonString());

        await ChangeAsync("Meal", definitionId, provisionId, WithRules(form.ToJsonString(), new JsonArray()));
        Assert.Null((await FormAsync("Meal", definitionId, provisionId))["form"]!["settlement"]!["cancellationRules"]);
        Assert.Equal(
            new[] { "0|0" },
            await DeskAsync(async desk => await desk.Scope.Command.Database
                .SqlQuery<string>($"SELECT CONCAT((SELECT COUNT(*) FROM Ancillary.ProvisionCancellationRules WHERE AncillaryProvisionId = {provisionId}), '|', (SELECT COUNT(*) FROM ReadModel.AncillaryProvisionCancellationRules WHERE AncillaryProvisionId = {provisionId})) AS Value")
                .ToListAsync()));
    }

    [Fact]
    public async Task PROF_05_an_old_seat_body_keeps_the_rules_filed_on_its_provision_and_its_four_routes_take_the_same_settlement()
    {
        var (definitionId, rule) = await DefinitionAsync("A07");
        var oldBody = JsonSerializer.Serialize(new SeatProvisionRequest(
            rule.Sequence, rule.CoverageScope, rule.PurchaseStage, rule.PriceOrigin, rule.QuoteProviderKey, rule.Quantity, rule.Outcome, rule.Settlement, rule.Availability,
            rule.Fulfillment, rule.PassengerEligibility, rule.SalesRestrictions, rule.Geography, rule.FlightApplication, rule.FareApplication, rule.TravelDate,
            rule.DayTimeApplication, rule.AdvancePurchase, rule.SeatApplication), HostJson.Options);
        var withoutRules = JsonNode.Parse(oldBody)!;

        withoutRules["settlement"]!.AsObject().Remove("cancellationRules");

        var filed = WithRules(withoutRules.ToJsonString(), Rules(Rule("SEAT-VOL", "Voluntary", null, null, "None")));
        var provisionId = await DeskAsync(async desk => long.Parse(HostJson.Data(
            await desk.SeatProvisions.Define(definitionId, HostJson.Bind<SeatProvisionRequest>(filed), CancellationToken.None))["id"]!.GetValue<string>()));

        await DeskAsync(async desk => HostJson.Data(await desk.SeatProvisions.Change(
            definitionId,
            provisionId,
            HostJson.Bind<SeatProvisionRequest>(withoutRules.ToJsonString()),
            CancellationToken.None)));

        var form = await DeskAsync(async desk => HostJson.Data(await desk.SeatProvisions.GetForm(definitionId, provisionId, CancellationToken.None)));

        Assert.Equal("SEAT-VOL", form["form"]!["settlement"]!["cancellationRules"]![0]!["ruleRef"]!.GetValue<string>());
        Assert.Equal(
            new[] { "reissueRefund", "formOfRefund", "commissionable", "interlineSettlement", "cancellationRules" },
            Members(form["form"]!["settlement"]!));
    }

    [Fact]
    public async Task PROF_06_a_typed_provision_is_published_without_cancellation_rules_through_the_shared_pricing_and_lifecycle()
    {
        var (freeId, free) = await DefinitionAsync("A11", activate: true);
        var (paidId, paid) = await DefinitionAsync("A12", activate: true);
        var freeProvisionId = await DefineAsync("Meal", freeId, Body("Meal", free));
        var paidProvisionId = await DefineAsync("Meal", paidId, Body("Meal", paid));
        var pricingId = await DeskAsync(async desk => (await desk.Scope.DefinePricing.DefineAsync(Pricing(paidProvisionId, P1Commands.Eur, Base(12m, name: "Meal")))).Id);

        await DeskAsync(desk => desk.Scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(freeProvisionId)));
        await DeskAsync(desk => desk.Scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(paidProvisionId, pricingId)));

        foreach (var (definitionId, provisionId) in new[] { (freeId, freeProvisionId), (paidId, paidProvisionId) })
        {
            var view = await FormAsync("Meal", definitionId, provisionId);

            Assert.Equal(("Active", false), (view["status"]!["name"]!.GetValue<string>(), view["editable"]!.GetValue<bool>()));
            Assert.Null(view["form"]!["settlement"]!["cancellationRules"]);
        }

        Assert.Equal(new[] { "Free", "NotAvailable" }, (await FormAsync("Meal", freeId, freeProvisionId))["constraints"]!["allowedDispositions"]!.AsArray().Select(value => value!["name"]!.GetValue<string>()));
        await RefusedAsync(16321, 409, desk => CallAsync(desk, "Meal", "Define", freeId, Bind("Meal", Body("Meal", paid with { ServiceDefinitionId = freeId }))));
    }

    private static string StoresText((List<string> Command, List<string> ReadModel) rows) => $"{string.Join(",", rows.Command)} | {string.Join(",", rows.ReadModel)}";

    private async Task<(List<string> Command, List<string> ReadModel)> RulesInBothStoresAsync(long provisionId)
        => ((await DeskAsync(desk => desk.Scope.Command.Database
                .SqlQuery<string>($"SELECT CONCAT(Id, ':', RuleRef, ':', COALESCE(CONVERT(varchar(30), CONVERT(decimal(19, 2), PenaltyFixedAmount)), '-'), ':', COALESCE(CONVERT(varchar(30), CONVERT(decimal(9, 2), PenaltyPercentage)), '-')) AS Value FROM Ancillary.ProvisionCancellationRules WHERE AncillaryProvisionId = {provisionId}")
                .ToListAsync())).OrderBy(row => row, StringComparer.Ordinal).ToList(),
            (await DeskAsync(desk => desk.Scope.Command.Database
                .SqlQuery<string>($"SELECT CONCAT(Id, ':', RuleRef, ':', COALESCE(CONVERT(varchar(30), CONVERT(decimal(19, 2), PenaltyFixedAmount)), '-'), ':', COALESCE(CONVERT(varchar(30), CONVERT(decimal(9, 2), PenaltyPercentage)), '-')) AS Value FROM ReadModel.AncillaryProvisionCancellationRules WHERE AncillaryProvisionId = {provisionId}")
                .ToListAsync())).OrderBy(row => row, StringComparer.Ordinal).ToList());

    [Fact]
    public async Task PROF_07_R1_a_rule_changed_under_its_reference_keeps_its_row_in_both_stores_and_the_reference_ignores_case()
    {
        var (definitionId, rule) = await DefinitionAsync("A12");
        var provisionId = await DefineAsync("Meal", definitionId, WithRules(Body("Meal", rule), Rules(
            Rule("VOL-24H", "Voluntary", 1440, null, "FixedAmount", amount: 15m),
            Rule("VOL-LATE", "Voluntary", 0, 1440, "Percentage", percentage: 40m, percentageBase: "ServiceTotal"))));
        var form = (await FormAsync("Meal", definitionId, provisionId))["form"]!.ToJsonString();
        var ids = (await DeskAsync(desk => desk.Scope.Command.Set<ProvisionCancellationRule>()
                .AsNoTracking().Where(row => row.AncillaryProvisionId == provisionId).ToListAsync()))
            .ToDictionary(row => row.RuleRef, row => row.Id);

        await ChangeAsync("Meal", definitionId, provisionId, WithRules(form, Rules(
            Rule("VOL-24H", "Voluntary", 1440, null, "FixedAmount", amount: 20m),
            Rule("vol-late", "Voluntary", 0, 1440, "Percentage", percentage: 50m, percentageBase: "ServiceTotal"))));

        var (command, readModel) = await RulesInBothStoresAsync(provisionId);

        Assert.Equal(new[] { $"{ids["VOL-24H"]}:VOL-24H:20.00:-", $"{ids["VOL-LATE"]}:vol-late:-:50.00" }.OrderBy(row => row, StringComparer.Ordinal), command);
        Assert.Equal(command, readModel);

        var typed = (await FormAsync("Meal", definitionId, provisionId))["form"]!["settlement"]!["cancellationRules"]!;
        var generic = (await GenericFormAsync(provisionId))["form"]!["settlement"]!["cancellationRules"]!;
        var byId = await DeskAsync(desk => desk.Scope.GetProvisionById.ExecuteAsync(provisionId));

        Assert.Equal(new[] { "VOL-24H", "vol-late" }, typed.AsArray().Select(value => value!["ruleRef"]!.GetValue<string>()));
        Assert.Equal(typed.ToJsonString(), generic.ToJsonString());
        Assert.Equal(new[] { (20m, (decimal?)null), (0m, 50m) }, byId.CancellationRules.Select(row => (row.PenaltyFixedAmount ?? 0m, row.PenaltyPercentage)));

        await RefusedAsync(16302, 422, desk => CallAsync(desk, "Meal", "Change", definitionId, provisionId, Bind("Meal", WithRules(form, Rules(
            Rule("NOSHOW", "NoShow", null, null, "ManualReview"),
            Rule("noshow", "Involuntary", null, null, "ManualReview"))))));
        Assert.Equal(StoresText((command, readModel)), StoresText(await RulesInBothStoresAsync(provisionId)));

        await ChangeAsync("Meal", definitionId, provisionId, WithRules(form, Rules(Rule("SUPPLIER", "SupplierFailure", null, null, "ProviderQuoted"))));

        var replaced = await RulesInBothStoresAsync(provisionId);

        Assert.Single(replaced.Command);
        Assert.DoesNotContain(replaced.Command, row => ids.Values.Any(id => row.StartsWith($"{id}:", StringComparison.Ordinal)));
        Assert.Equal(replaced.Command, replaced.ReadModel);

        await ChangeAsync("Meal", definitionId, provisionId, WithRules(form, new JsonArray()));

        Assert.Equal(" | ", StoresText(await RulesInBothStoresAsync(provisionId)));
    }

    [Fact]
    public async Task PROF_08_R2_a_refused_change_leaves_the_stored_draft_and_its_form_as_they_were()
    {
        var (freeMealId, freeMeal) = await DefinitionAsync("A11");
        var (wifiId, wifi) = await DefinitionAsync("A24");
        var filed = Rules(Rule("VOL-24H", "Voluntary", 1440, null, "FixedAmount", amount: 15m));
        var freeMealProvisionId = await DefineAsync("Meal", freeMealId, WithRules(Body("Meal", freeMeal), filed));
        var wifiProvisionId = await DefineAsync("Connectivity", wifiId, WithRules(Body("Connectivity", wifi), filed));

        foreach (var (profile, definitionId, provisionId, edit, code, status) in new (string, long, long, Action<JsonNode>, int, int)[]
        {
            ("Meal", freeMealId, freeMealProvisionId, body => { body["outcome"]!["disposition"] = "Paid"; body["priceOrigin"] = "Filed"; }, 16321, 409),
            ("Connectivity", wifiId, wifiProvisionId, body => body["purchaseStage"] = "OnBoard", 16323, 409),
            ("Meal", freeMealId, freeMealProvisionId, body => body["settlement"]!["cancellationRules"] = Rules(Rule("BAD", "Voluntary", 0, 10, "Percentage", percentage: 0m, percentageBase: "ServiceBase")), 16302, 422),
            ("Connectivity", wifiId, wifiProvisionId, body => body["quantity"]!["unit"] = "Piece", 16312, 409)
        })
        {
            var before = HostJson.Text(await FormAsync(profile, definitionId, provisionId));
            var rows = await RulesInBothStoresAsync(provisionId);
            var body = JsonNode.Parse((await FormAsync(profile, definitionId, provisionId))["form"]!.ToJsonString())!;

            body["sequence"] = 77;
            body["settlement"]!["cancellationRules"] = Rules(Rule("VOL-24H", "Voluntary", 1440, null, "FixedAmount", amount: 99m));
            edit(body);

            await RefusedAsync(code, status, desk => CallAsync(desk, profile, "Change", definitionId, provisionId, Bind(profile, body.ToJsonString())));

            Assert.Equal(before, HostJson.Text(await FormAsync(profile, definitionId, provisionId)));
            Assert.Equal(StoresText(rows), StoresText(await RulesInBothStoresAsync(provisionId)));
        }

        var genericBaggage = freeMeal with { ApplicationType = ProvisionApplicationType.Baggage, BaggageApplication = P1Commands.Baggage(23m, 1, 1) };

        await BusinessAssert.ThrowsAsync(16321, 409, () => DeskAsync(desk => desk.Scope.DefineProvision.DefineAsync(genericBaggage)));
    }

    [Fact]
    public async Task PROF_09_R4_a_provision_without_a_filed_penalty_currency_is_authored_without_the_currency_reference()
    {
        var (definitionId, rule) = await DefinitionAsync("A12");
        var unreachable = new UnreachableCurrencies();

        async Task<long> DefineWithoutReferenceAsync(TestDefineProvisionCommand command)
        {
            await using var scope = new AncillaryScope(_database, _clock);
            var service = new DefineAncillaryProvisionService(
                scope.Provisions,
                scope.Definitions,
                new AncillaryProvisionQueryDbSynchronizer(scope.Query, _clock),
                unreachable,
                scope.UnitOfWork,
                _database.Ids,
                _clock);

            return (await service.DefineAsync(command)).Id;
        }

        var withoutRules = await DefineWithoutReferenceAsync(rule);
        var percentageOnly = await DefineWithoutReferenceAsync(rule with
        {
            Sequence = rule.Sequence + 1,
            Settlement = rule.Settlement with
            {
                CancellationRules = [new ProvisionCancellationRuleInput("VOL", CancellationReasonKind.Voluntary, 0, null, PenaltyKind.Percentage, null, 25m, null, CancellationPercentageBase.ServiceBase, false, "CARRIER-FILED-TERMS")]
            }
        });

        Assert.True(withoutRules > 0 && percentageOnly > 0);
        Assert.Equal(0, unreachable.Calls);
        await Assert.ThrowsAsync<InvalidOperationException>(() => DefineWithoutReferenceAsync(rule with
        {
            Sequence = rule.Sequence + 2,
            Settlement = rule.Settlement with
            {
                CancellationRules = [new ProvisionCancellationRuleInput("VOL", CancellationReasonKind.Voluntary, 0, null, PenaltyKind.FixedAmount, 5m, null, P1Commands.Eur, null, false, "CARRIER-FILED-TERMS")]
            }
        }));
        Assert.Equal(1, unreachable.Calls);
    }

    private sealed class UnreachableCurrencies : ICurrencyReference
    {
        public int Calls { get; private set; }

        public Task<IReadOnlyDictionary<int, int>> FindDecimalPlacesAsync(IReadOnlyCollection<int> currencyIds, CancellationToken cancellationToken = default)
        {
            Calls++;

            throw new InvalidOperationException("The currency reference was called.");
        }

        public Task<IReadOnlyDictionary<int, string>> FindCodesAsync(IReadOnlyCollection<int> currencyIds, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("The currency reference was called.");
    }

    private sealed class FixedForm(BackofficeProvisionFormDto form) : IGetAncillaryProvisionFormService
    {
        public Task<BackofficeProvisionFormDto> ExecuteAsync(long provisionId, CancellationToken cancellationToken = default) => Task.FromResult(form);
    }
}
