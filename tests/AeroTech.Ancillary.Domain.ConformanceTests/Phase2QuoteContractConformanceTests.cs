using System.Text.Json;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using Xunit;
using YamlDotNet.Serialization;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public sealed class Phase2QuoteContractConformanceTests
{
    private readonly Dictionary<object, object> _schemas = Schemas();

    [Fact]
    public void P2_Q01_DocumentedLoungeItemUsesExactlyTheOpenApiMemberNames()
    {
        using var item = JsonDocument.Parse(RepositoryFiles.LoungeGoldenExample().LoungeItem);

        Assert.Equal(Properties("QuoteItem"), Members(item.RootElement));
        Assert.Equal(Properties("LoungeDetail"), Members(item.RootElement.GetProperty("lounge")));
        Assert.Equal(Properties("IndustryCodes"), Members(item.RootElement.GetProperty("codes")));
        Assert.Equal(Properties("DocumentPolicy"), Members(item.RootElement.GetProperty("document")));
        Assert.Equal(JsonValueKind.Null, item.RootElement.GetProperty("baggage").ValueKind);
    }

    [Fact]
    public void P2_Q01_DocumentedGoldenRequestUsesOnlyOpenApiMemberNames()
    {
        using var request = JsonDocument.Parse(RepositoryFiles.LoungeGoldenExample().Request);
        var root = request.RootElement;

        Assert.Empty(Members(root).Except(Properties("AncillaryQuoteRequest"), StringComparer.Ordinal));
        Assert.Empty(Members(root.GetProperty("travellers")[0]).Except(Properties("Traveller"), StringComparer.Ordinal));
        Assert.Empty(Members(root.GetProperty("bounds")[1]).Except(Properties("Bound"), StringComparer.Ordinal));
        Assert.Empty(Members(root.GetProperty("bounds")[1].GetProperty("flights")[0]).Except(Properties("Flight"), StringComparer.Ordinal));
    }

    private IEnumerable<string> Properties(string schema)
        => ((Dictionary<object, object>)((Dictionary<object, object>)_schemas[schema])["properties"])
            .Keys
            .Cast<string>()
            .Order(StringComparer.Ordinal);

    private static IEnumerable<string> Members(JsonElement element)
        => element.EnumerateObject().Select(member => member.Name).Order(StringComparer.Ordinal);

    private static Dictionary<object, object> Schemas()
    {
        var contract = new DeserializerBuilder().Build().Deserialize<Dictionary<object, object>>(RepositoryFiles.QuoteContractText());

        return (Dictionary<object, object>)((Dictionary<object, object>)contract["components"])["schemas"];
    }
}
