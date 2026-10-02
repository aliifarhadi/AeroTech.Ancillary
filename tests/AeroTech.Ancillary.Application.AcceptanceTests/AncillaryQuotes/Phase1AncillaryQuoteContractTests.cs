using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Query.AncillaryQuote.Dto;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote.Service;
using AeroTech.Framework.Presentation.Responses;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.AncillaryQuotes;

[Collection(DatabaseCollection.Name)]
public sealed class Phase1AncillaryQuoteContractTests(TestDatabase database)
{
    private const int GoldenAirlineId = 10;
    private const string RuleIdPlaceholder = "{ruleId}";

    private readonly AncillaryHarness _harness = new(database);

    [Fact]
    public async Task P1_Q01_GoldenExampleGivesExactlyTheDocumentedResponse()
    {
        await _harness.RegisterAsync(Phase1Commands.SubCodeS(GoldenAirlineId));
        await _harness.ArrangeProductAsync(Phase1Commands.ProductX(GoldenAirlineId));

        var rule = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(GoldenAirlineId));
        var ruleId = rule.Id.ToString(CultureInfo.InvariantCulture);
        var (request, responseData) = RepositoryFiles.GoldenExample();
        var selectionQuery = ApiJson.Deserialize<ServiceGetAncillaryQuoteQuery>(request.Replace(RuleIdPlaceholder, ruleId));
        var expected = JsonNode.Parse(responseData.Replace(RuleIdPlaceholder, ruleId));

        var selected = await _harness.QuoteAsync(selectionQuery);
        var catalogue = await _harness.QuoteAsync(selectionQuery with { Selections = null });

        var selectedBody = Body(selected);
        var catalogueBody = Body(catalogue);

        ApiJson.AssertSame(expected, selectedBody["data"]);
        ApiJson.AssertSame(expected, catalogueBody["data"]);
        Assert.Equal(["data", "errors"], selectedBody.Select(member => member.Key).Order(StringComparer.Ordinal));
        Assert.Null(selectedBody["errors"]);
        Assert.Equal(JsonValueKind.String, selectedBody["data"]!["items"]![0]!["priceRuleId"]!.GetValueKind());
        Assert.Equal(ruleId, selectedBody["data"]!["items"]![0]!["priceRuleId"]!.GetValue<string>());
    }

    private static JsonObject Body(AncillaryQuoteDto quote)
        => JsonNode.Parse(JsonSerializer.Serialize(new ApiResult<AncillaryQuoteDto>(quote), ApiJson.Options))!.AsObject();
}
