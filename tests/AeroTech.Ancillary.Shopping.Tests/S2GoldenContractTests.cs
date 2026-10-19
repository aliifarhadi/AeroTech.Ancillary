using System.Text.Json.Nodes;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2GoldenContractTests
{
    public static TheoryData<string> Files =>
    [
        "backoffice-offers.json", "backoffice-detail.json", "backoffice-selection.json",
        "api-offers.json", "api-detail.json", "api-selection.json",
        "otapanel-offers.json", "otapanel-detail.json", "otapanel-selection.json"
    ];

    [Theory]
    [MemberData(nameof(Files))]
    public void S2_GOLD_01_every_member_published_by_s2_01_is_still_published_and_only_additions_are_new(string file)
    {
        var before = Paths(Read("S201", file));
        var after = Paths(Read(null, file));

        Assert.Empty(before.Except(after));
    }

    private static JsonNode Read(string? folder, string file)
        => JsonNode.Parse(File.ReadAllText(folder is null
            ? Path.Combine(AppContext.BaseDirectory, "Fixtures", "Golden", file)
            : Path.Combine(AppContext.BaseDirectory, "Fixtures", "Golden", folder, file)))!;

    private static HashSet<string> Paths(JsonNode node)
    {
        var paths = new HashSet<string>(StringComparer.Ordinal);

        void Walk(JsonNode? current, string path)
        {
            switch (current)
            {
                case JsonObject members:
                    foreach (var (name, value) in members)
                    {
                        var key = path.EndsWith(".bags", StringComparison.Ordinal) || path.EndsWith(".other-services", StringComparison.Ordinal) ? "{ref}" : name;

                        paths.Add($"{path}.{key}");
                        Walk(value, $"{path}.{key}");
                    }

                    break;
                case JsonArray items:
                    foreach (var item in items)
                        Walk(item, $"{path}[]");

                    break;
            }
        }

        Walk(node, "$");

        return paths;
    }
}
