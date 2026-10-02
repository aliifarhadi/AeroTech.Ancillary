using System.Text.Json;
using System.Text.RegularExpressions;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using YamlDotNet.Serialization;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public sealed partial class Phase1QuoteContractConformanceTests
{
    private readonly IReadOnlyDictionary<string, Dictionary<object, object>> _schemas = Schemas();

    [Fact]
    public void P1_Q01_OpenApiEnumValueNamesAreTheContractEnumNames()
    {
        var openEnums = _schemas.Values
            .SelectMany(schema => Properties(schema).Values)
            .Select(property => property.TryGetValue("description", out var description) ? description as string : null)
            .Select(description => description is null ? null : OpenEnum().Match(description))
            .Where(match => match is { Success: true })
            .Select(match => (Enum: match!.Groups["enum"].Value, Names: match.Groups["names"].Value.Split(", ")))
            .DistinctBy(openEnum => openEnum.Enum)
            .ToList();

        Assert.Equal(
            ["AncillaryDocumentType", "AncillaryInventoryControl", "AncillaryPriceLineCategory", "AncillaryProductType", "AncillaryQuantityUnit", "AncillarySalesScope", "AncillaryWeightUnit"],
            openEnums.Select(openEnum => openEnum.Enum).Order(StringComparer.Ordinal));

        foreach (var (name, names) in openEnums)
        {
            var type = typeof(AncillaryProductType).Assembly.GetType($"{typeof(AncillaryProductType).Namespace}.{name}")!;

            Assert.Equal(names.Order(StringComparer.Ordinal), Enum.GetNames(type).Order(StringComparer.Ordinal));
        }
    }

    [Fact]
    public void P1_Q01_DocumentedGoldenResponseUsesExactlyTheOpenApiMemberNames()
    {
        using var response = JsonDocument.Parse(RepositoryFiles.GoldenExample().ResponseData);
        var data = response.RootElement;
        var item = data.GetProperty("items")[0];

        AssertMembers("AncillaryQuote", data);
        AssertMembers("QuoteItem", item);
        AssertMembers("IndustryCodes", item.GetProperty("codes"));
        AssertMembers("BaggageDetail", item.GetProperty("baggage"));
        AssertMembers("SalesTerms", item.GetProperty("terms"));
        AssertMembers("DocumentPolicy", item.GetProperty("document"));
        AssertMembers("Inventory", item.GetProperty("inventory"));

        foreach (var line in item.GetProperty("priceLines").EnumerateArray())
            AssertMembers("QuotePriceLine", line);
    }

    [Fact]
    public void P1_Q01_DocumentedGoldenRequestUsesOnlyOpenApiMemberNames()
    {
        using var request = JsonDocument.Parse(RepositoryFiles.GoldenExample().Request);
        var root = request.RootElement;

        AssertKnownMembers("AncillaryQuoteRequest", root);
        AssertKnownMembers("SalesContext", root.GetProperty("salesContext"));
        AssertKnownMembers("Traveller", root.GetProperty("travellers")[0]);
        AssertKnownMembers("Bound", root.GetProperty("bounds")[0]);
        AssertKnownMembers("Flight", root.GetProperty("bounds")[0].GetProperty("flights")[0]);
        AssertKnownMembers("Selection", root.GetProperty("selections")[0]);
    }

    private void AssertMembers(string schema, JsonElement element)
        => Assert.Equal(Properties(_schemas[schema]).Keys.Order(StringComparer.Ordinal), Members(element));

    private void AssertKnownMembers(string schema, JsonElement element)
        => Assert.Empty(Members(element).Except(Properties(_schemas[schema]).Keys, StringComparer.Ordinal));

    private static IEnumerable<string> Members(JsonElement element)
        => element.EnumerateObject().Select(member => member.Name).Order(StringComparer.Ordinal);

    private static Dictionary<string, Dictionary<object, object>> Properties(Dictionary<object, object> schema)
        => ((Dictionary<object, object>)schema["properties"]).ToDictionary(property => (string)property.Key, property => (Dictionary<object, object>)property.Value);

    private static IReadOnlyDictionary<string, Dictionary<object, object>> Schemas()
    {
        var contract = new DeserializerBuilder().Build().Deserialize<Dictionary<object, object>>(RepositoryFiles.QuoteContractText());
        var schemas = (Dictionary<object, object>)((Dictionary<object, object>)contract["components"])["schemas"];

        return schemas
            .Where(schema => ((Dictionary<object, object>)schema.Value).ContainsKey("properties"))
            .ToDictionary(schema => (string)schema.Key, schema => (Dictionary<object, object>)schema.Value);
    }

    [GeneratedRegex(@"Open enum (?<enum>\w+)\. Known(?: in Phase 1)?: (?<names>[\w, ]+)\.")]
    private static partial Regex OpenEnum();
}
