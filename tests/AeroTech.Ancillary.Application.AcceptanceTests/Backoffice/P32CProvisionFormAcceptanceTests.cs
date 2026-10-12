using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Backoffice;

[Collection(DatabaseCollection.Name)]
public class P32CProvisionFormAcceptanceTests
{
    private static readonly string[] EditViewHeader =
    [
        "id", "serviceDefinitionId", "status", "profile", "variantCode", "applicableRuleSections", "createdAt", "activatedAt", "suspendedAt", "retiredAt",
        "form"
    ];

    private static readonly string[] RuleGroups =
    [
        "passengerEligibility", "salesRestrictions", "geography", "flightApplication", "fareApplication", "travelDate", "dayTimeApplication",
        "advancePurchase", "baggageApplication"
    ];

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public P32CProvisionFormAcceptanceTests(TestDatabase database) => _database = database;

    public static TheoryData<string> Variants => [.. AncillaryVariant.All.Select(variant => variant.Code).Order(StringComparer.Ordinal)];

    private static string[] Members(JsonNode node) => node.AsObject().Select(member => member.Key).ToArray();

    private static JsonNode WithoutRowIds(JsonNode node, bool root = true)
    {
        switch (node)
        {
            case JsonObject item:
                var copy = new JsonObject();

                foreach (var member in item.Where(member => root || member.Key != "id"))
                    copy[member.Key] = member.Value is null ? null : WithoutRowIds(member.Value, false);

                return copy;
            case JsonArray items:
                return new JsonArray(items.Select(entry => entry is null ? null : WithoutRowIds(entry, false)).ToArray());
            default:
                return node.DeepClone();
        }
    }

    private static TestDefineProvisionCommand Rich(TestDefineProvisionCommand rule)
        => rule with
        {
            PassengerEligibility = new ProvisionPassengerEligibilityInput(
                [PassengerTypeCode.ADT, PassengerTypeCode.CHD],
                [new ProvisionAgeBandInput(2, 12), new ProvisionAgeBandInput(18, null)]),
            SalesRestrictions = new ProvisionSalesRestrictionsInput(
                new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2027, 11, 1, 0, 0, 0, TimeSpan.Zero),
                [V122Catalog.PointOfSale],
                [501, 502],
                [Enum.GetValues<CustomerType>()[0]]),
            Geography = new ProvisionGeographyInput(
                [P1Commands.Thr],
                [P1Commands.Ika],
                null,
                [new ProvisionRoutePairInput(P1Commands.Thr, P1Commands.Ika, Enum.GetValues<RoutePairDirection>()[0])],
                null,
                null),
            FlightApplication = new ProvisionFlightApplicationInput([1], [1], ["111", "114"], null, [1, 3]),
            FareApplication = new ProvisionFareApplicationInput(null, [Enum.GetValues<AirFareType>()[0]], [1, 2], ["YOW"], [1], [11]),
            TravelDate = new ProvisionTravelDateInput(
                [new ProvisionDatePeriodInput(new DateOnly(2027, 1, 1), new DateOnly(2027, 12, 31))],
                [new ProvisionDatePeriodInput(new DateOnly(2027, 3, 18), new DateOnly(2027, 3, 25))]),
            DayTimeApplication = new ProvisionDayTimeApplicationInput(
                [new ProvisionDayTimeWindowInput(31, new TimeOnly(6, 0), new TimeOnly(22, 0), DayTimeRestrictionEffect.Allow)]),
            AdvancePurchase = new ProvisionAdvancePurchaseInput(24, TimeUnit.Hours, false, 330)
        };

    private async Task<long> ProvisionAsync(string variantCode, Func<TestDefineProvisionCommand, TestDefineProvisionCommand>? shape = null)
    {
        var airlineId = _database.NextAirlineId();
        var variant = V122Catalog.Case(variantCode);

        await using var desk = new ControllerDesk(_database, _clock);

        var supplierId = (await desk.Scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId))).Id;
        var definitionId = (await desk.Scope.DefineServiceDefinition.DefineAsync(V122Catalog.Define(variant, airlineId, supplierId))).Id;
        var rule = V122Catalog.Rule(variant, definitionId);

        return (await desk.Scope.DefineProvision.DefineAsync(shape is null ? rule : shape(rule))).Id;
    }

    private async Task<JsonNode> ReadAsync(long provisionId)
    {
        await using var desk = new ControllerDesk(_database, _clock);

        return HostJson.Data(await desk.Provisions.GetById(provisionId, CancellationToken.None));
    }

    private async Task<JsonNode> FormAsync(long provisionId)
    {
        await using var desk = new ControllerDesk(_database, _clock);

        return HostJson.Data(await desk.Provisions.GetForm(provisionId, CancellationToken.None));
    }

    private async Task<JsonNode> SaveAsync(long provisionId, string body)
    {
        await using var desk = new ControllerDesk(_database, _clock);

        return HostJson.Data(await desk.Provisions.Change(provisionId, HostJson.Bind<ChangeProvisionRequest>(body), CancellationToken.None));
    }

    private async Task<JsonNode> AssertSavesBackWithoutChangeAsync(long provisionId)
    {
        var before = await ReadAsync(provisionId);
        var view = await FormAsync(provisionId);
        var form = view["form"]!;

        Assert.Equal(EditViewHeader, Members(view));
        Assert.Equal(typeof(ChangeProvisionRequest).GetProperties().Select(member => JsonNamingPolicy.CamelCase.ConvertName(member.Name)), Members(form));

        await SaveAsync(provisionId, form.ToJsonString());

        Assert.Equal(HostJson.Text(WithoutRowIds(before)), HostJson.Text(WithoutRowIds(await ReadAsync(provisionId))));
        Assert.Equal(HostJson.Text(view), HostJson.Text(await FormAsync(provisionId)));

        return view;
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public async Task P32C_PRV_01_the_form_of_a_provision_is_saved_back_and_no_stored_value_changes(string variantCode)
    {
        var view = await AssertSavesBackWithoutChangeAsync(await ProvisionAsync(variantCode));

        Assert.Equal(variantCode, view["variantCode"]!.GetValue<string>());
        Assert.Equal("Draft", view["status"]!["name"]!.GetValue<string>());
        Assert.Equal(V122Catalog.PointOfSale.ToString(), view["form"]!["salesRestrictions"]!["allowedPointOfSaleIds"]![0]!.GetValue<string>());
    }

    [Fact]
    public async Task P32C_PRV_02_a_provision_with_every_shared_rule_group_is_saved_back_and_no_stored_value_changes()
    {
        var view = await AssertSavesBackWithoutChangeAsync(await ProvisionAsync("A01", Rich));
        var form = view["form"]!;

        Assert.All(RuleGroups, group => Assert.NotNull(form[group]));
        Assert.Equal(2, form["passengerEligibility"]!["allowedAgeBands"]!.AsArray().Count);
        Assert.Equal("ADT", form["passengerEligibility"]!["allowedPassengerTypes"]![0]!.GetValue<string>());
        Assert.Equal(2, form["salesRestrictions"]!["allowedCustomerIds"]!.AsArray().Count);
        Assert.Single(form["geography"]!["allowedRoutePairs"]!.AsArray());
        Assert.Equal(2, form["flightApplication"]!["allowedFlightNumbers"]!.AsArray().Count);
        Assert.Single(form["travelDate"]!["blackoutPeriods"]!.AsArray());
        Assert.Equal(31, form["dayTimeApplication"]!["windows"]![0]!["daysOfWeekMask"]!.GetValue<int>());
        Assert.Equal(330, form["advancePurchase"]!["maximumPeriod"]!.GetValue<int>());
    }

    [Fact]
    public async Task P32C_PRV_03_the_form_of_an_unknown_provision_is_refused_like_the_read_by_id()
    {
        var expected = await Assert.ThrowsAsync<BusinessException>(() => ReadAsync(long.MaxValue));
        var actual = await Assert.ThrowsAsync<BusinessException>(() => FormAsync(long.MaxValue));

        Assert.Equal(404, actual.HttpStatus);
        Assert.Equal(expected.Code, actual.Code);
    }

    [Theory]
    [InlineData("A08")]
    [InlineData("A14")]
    public async Task P32C_PRV_04_the_published_examples_are_what_the_serializer_writes(string variantCode)
    {
        var provisionId = await ProvisionAsync(variantCode);
        var view = await FormAsync(provisionId);
        var response = await SaveAsync(provisionId, view["form"]!.ToJsonString());
        var example = new JsonObject
        {
            ["read"] = view.DeepClone(),
            ["editRequest"] = view["form"]!.DeepClone(),
            ["editResponse"] = response.DeepClone()
        };
        var actual = HostJson.Text(example)
            .Replace(provisionId.ToString(), "{provisionId}")
            .Replace(view["serviceDefinitionId"]!.GetValue<string>(), "{serviceDefinitionId}");
        var file = RepositoryFiles.TestProjectFile(Path.Combine("Fixtures", "Contracts", $"provision-form-{variantCode}.json"));

        if (Environment.GetEnvironmentVariable("P32C_WRITE_EXAMPLES") == "1")
            await File.WriteAllTextAsync(file, actual.ReplaceLineEndings("\r\n"));
        else
            Assert.Equal((await File.ReadAllTextAsync(file)).ReplaceLineEndings("\n"), actual);
    }
}
