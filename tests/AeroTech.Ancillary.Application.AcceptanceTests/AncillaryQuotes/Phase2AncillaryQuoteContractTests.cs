using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Query.AncillaryQuote.Dto;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote.Service;
using AeroTech.Framework.Presentation.Responses;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.AncillaryQuotes;

[Collection(DatabaseCollection.Name)]
public sealed class Phase2AncillaryQuoteContractTests(TestDatabase database)
{
    private const string DocumentedAirlineId = "10";

    private readonly AncillaryHarness _harness = new(database);

    private int Airline => _harness.AirlineId;

    [Fact]
    public async Task P2_Q01_GoldenExampleGivesExactlyTheDocumentedItems()
    {
        await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));
        await _harness.RegisterAsync(Phase2Commands.SubCodeL(Airline));
        await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));
        await _harness.ArrangeProductAsync(Phase2Commands.ProductL(Airline));

        var bagRule = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));
        var loungeRule = await _harness.ArrangePriceRuleAsync(Phase2Commands.RuleRL(Airline));
        var (request, loungeItem) = RepositoryFiles.LoungeGoldenExample();
        var bagItem = JsonNode.Parse(RepositoryFiles.GoldenExample().ResponseData.Replace("{ruleId}", Id(bagRule.Id)))!["items"]![0]!;
        var secondBagItem = bagItem.DeepClone();

        secondBagItem["boundRef"] = "B2";
        secondBagItem["coveredFlightRefs"] = new JsonArray("F2");

        var expected = JsonNode.Parse(new JsonArray(
            ForAirline(bagItem.DeepClone()),
            ForAirline(JsonNode.Parse(loungeItem.Replace("{ruleIdRL}", Id(loungeRule.Id)))!),
            ForAirline(secondBagItem)).ToJsonString());
        var query = ApiJson.Deserialize<ServiceGetAncillaryQuoteQuery>(
            request.Replace($"\"marketingAirlineId\": {DocumentedAirlineId}", $"\"marketingAirlineId\": {Airline}"));

        var body = JsonNode.Parse(JsonSerializer.Serialize(new ApiResult<AncillaryQuoteDto>(await _harness.QuoteAsync(query)), ApiJson.Options))!;

        ApiJson.AssertSame(expected, body["data"]!["items"]);
        Assert.Equal(978, body["data"]!["currencyId"]!.GetValue<int>());
        Assert.Null(body["errors"]);
    }

    [Fact]
    public void P2_G02_Phase1ProofRequestThirtyIsStillMalformed()
    {
        var body = RepositoryFiles.ProofRequestBody("phase-1.http", 30).Replace("{{airlineId}}", "10");

        Assert.Contains("\"inventoryControl\": \"Quota\"", body);
        ApiJson.Malformed<BackofficeDefineAncillaryProductCommand>(body);
    }

    private JsonNode ForAirline(JsonNode item)
    {
        item["ownerAirlineId"] = Airline;

        return item;
    }

    private static string Id(long value) => value.ToString(CultureInfo.InvariantCulture);
}
