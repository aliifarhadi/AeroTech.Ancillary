using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using NJsonSchema;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Backoffice;

public class P32COpenApiConformanceAcceptanceTests
{
    private const string Definitions = "/Backoffice/v1/AncillaryServiceDefinitions";

    public static TheoryData<string> Variants => [.. AncillaryVariant.All.Select(variant => variant.Code).Order(StringComparer.Ordinal)];

    public static TheoryData<string, string> ForeignVariantCodes => new()
    {
        { "A01", "A02" },
        { "A03", "A04" },
        { "A06", "A05" },
        { "A09", "A07" },
        { "A07", "A09" },
        { "A12", "A11" },
        { "A14", "A13" },
        { "A15", "A18" },
        { "A19", "A16" },
        { "A22", "A20" }
    };

    private static string Profile(string variantCode) => AncillaryVariant.Find(variantCode)!.Profile.ToString();

    private static JsonObject Example(string variantCode)
    {
        var profile = Profile(variantCode);
        var file = RepositoryFiles.TestProjectFile(
            Path.Combine("Fixtures", "Contracts", $"{char.ToLowerInvariant(profile[0])}{profile[1..]}-{variantCode}.json"));

        return JsonNode.Parse(File.ReadAllText(file))!.AsObject();
    }

    private static IEnumerable<JsonSchema> Reachable(JsonSchema schema, HashSet<JsonSchema> seen)
    {
        var actual = schema.ActualSchema;

        if (!seen.Add(actual))
            yield break;

        yield return actual;

        var children = actual.Properties.Values
            .Concat(actual.OneOf)
            .Concat(actual.AllOf)
            .Concat(actual.AnyOf)
            .Concat(actual.Item is null ? [] : [actual.Item]);

        foreach (var child in children.SelectMany(child => Reachable(child, seen)))
            yield return child;
    }

    private static void Collect(JsonNode? node, JsonObject schemas, SortedDictionary<string, JsonNode> used)
    {
        switch (node)
        {
            case JsonObject item:
                if (item["$ref"]?.GetValue<string>().Split('/')[^1] is { } name && !used.ContainsKey(name))
                {
                    used[name] = schemas[name]!;
                    Collect(schemas[name], schemas, used);
                }

                foreach (var member in item)
                    Collect(member.Value, schemas, used);

                break;
            case JsonArray items:
                foreach (var entry in items)
                    Collect(entry, schemas, used);

                break;
        }
    }

    [Theory]
    [MemberData(nameof(ForeignVariantCodes))]
    public async Task P32C_OAS_02_the_fields_of_one_variant_are_not_valid_under_the_code_of_another(string variantCode, string foreignCode)
    {
        var example = Example(variantCode);
        var collection = $"{Definitions}/{Profile(variantCode)}";
        var create = example["createRequest"]!.DeepClone();
        var edit = example["editRequest"]!.DeepClone();

        create["variantCode"] = foreignCode;
        edit["variantCode"] = foreignCode;

        Assert.NotEmpty(PublishedOpenApi.Errors(await PublishedOpenApi.RequestSchemaAsync("post", collection), create.ToJsonString()));
        Assert.NotEmpty(
            PublishedOpenApi.Errors(await PublishedOpenApi.RequestSchemaAsync("put", $"{collection}/{{serviceDefinitionId}}"), edit.ToJsonString()));
    }

    [Fact]
    public async Task P32C_OAS_03_every_choice_between_schemas_is_decided_by_the_variant_code()
    {
        var document = await PublishedOpenApi.DocumentAsync();
        var seen = new HashSet<JsonSchema>();
        var typed = document.Paths
            .Where(path => path.Key.StartsWith(Definitions, StringComparison.Ordinal))
            .SelectMany(path => path.Value.Values)
            .Where(operation => operation.Tags.Any(tag => tag.StartsWith("Backoffice ", StringComparison.Ordinal)))
            .ToList();
        var roots = typed
            .SelectMany(operation => new[]
            {
                operation.RequestBody?.Content["application/json"].Schema,
                operation.Responses["200"].Content["application/json"].Schema
            })
            .OfType<JsonSchema>()
            .ToList();
        var choices = roots.SelectMany(root => Reachable(root, seen)).Where(schema => schema.OneOf.Count > 0).ToList();

        Assert.Equal(31, typed.Count);
        Assert.Equal(18, choices.Count);
        Assert.All(choices, choice =>
        {
            Assert.Equal("variantCode", choice.DiscriminatorObject?.PropertyName);
            Assert.Equal(choice.OneOf.Count, choice.DiscriminatorObject!.Mapping.Count);
            Assert.All(choice.OneOf, alternative =>
            {
                var code = Assert.Single(alternative.ActualSchema.Properties["variantCode"].ActualSchema.Enumeration);

                Assert.Same(alternative.ActualSchema, choice.DiscriminatorObject.Mapping[(string)code!].ActualSchema);
            });
        });
        Assert.DoesNotContain(seen, schema => schema.AllOf.Any(part => part.ActualSchema.ActualProperties.Count == 0 && !part.ActualSchema.AllowAdditionalProperties));
    }

    [Theory]
    [InlineData("A08")]
    [InlineData("A14")]
    public async Task P32C_OAS_05_the_provision_form_examples_are_valid_against_the_published_schemas(string variantCode)
    {
        const string provision = "/Backoffice/v1/AncillaryProvisions/{provisionId}";

        var file = RepositoryFiles.TestProjectFile(Path.Combine("Fixtures", "Contracts", $"provision-form-{variantCode}.json"));
        var example = JsonNode.Parse(await File.ReadAllTextAsync(file))!;

        Assert.Empty(PublishedOpenApi.Errors(await PublishedOpenApi.AnswerSchemaAsync("get", $"{provision}/Form"), example["read"]!.ToJsonString()));
        Assert.Empty(PublishedOpenApi.Errors(await PublishedOpenApi.RequestSchemaAsync("put", provision), example["editRequest"]!.ToJsonString()));
    }

    [Fact]
    public async Task P32C_OAS_04_the_published_definition_contracts_are_the_recorded_ones()
    {
        var document = JsonNode.Parse(await PublishedOpenApi.TextAsync())!;
        var schemas = document["components"]!["schemas"]!.AsObject();
        var paths = new JsonObject();
        var used = new SortedDictionary<string, JsonNode>(StringComparer.Ordinal);

        foreach (var path in document["paths"]!.AsObject().Where(path => path.Key.StartsWith(Definitions, StringComparison.Ordinal)))
        {
            var operations = path.Value!.AsObject()
                .Where(operation => operation.Value!["tags"]!.AsArray().Any(tag => tag!.GetValue<string>().StartsWith("Backoffice ", StringComparison.Ordinal)))
                .ToList();

            if (operations.Count == 0)
                continue;

            paths[path.Key] = new JsonObject(operations.Select(operation => KeyValuePair.Create(operation.Key, (JsonNode?)operation.Value!.DeepClone())));
            Collect(paths[path.Key], schemas, used);
        }

        var recorded = new JsonObject
        {
            ["paths"] = paths,
            ["schemas"] = new JsonObject(used.Select(schema => KeyValuePair.Create(schema.Key, (JsonNode?)schema.Value.DeepClone())))
        };
        var actual = HostJson.Text(recorded);
        var file = RepositoryFiles.TestProjectFile(Path.Combine("Fixtures", "OpenApi", "definition-contracts.json"));

        if (Environment.GetEnvironmentVariable("P32C_WRITE_EXAMPLES") == "1")
            await File.WriteAllTextAsync(file, actual.ReplaceLineEndings("\r\n"));
        else
            Assert.Equal((await File.ReadAllTextAsync(file)).ReplaceLineEndings("\n"), actual);
    }
}
