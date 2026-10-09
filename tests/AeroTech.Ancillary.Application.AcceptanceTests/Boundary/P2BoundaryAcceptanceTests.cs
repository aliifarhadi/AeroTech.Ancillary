using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Framework.Core.Domain.Aggregates;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Boundary;

public class P2BoundaryAcceptanceTests
{
    private const string FrozenAggregate = "AncillaryReservationAggregate";
    private const string FrozenSourceHash = "6974ABA2C62BAFE7CDE159BDCE8A4969F1CAECCF25368012F30D1BE7DDA4E486";
    private const string CommandMigration = "20261008212718_V121Phase2StockCapacity.cs";
    private const string QueryMigration = "20261008212728_V121Phase2StockCapacityQuery.cs";

    private static readonly string Source = Path.Combine(RepositoryFiles.Root, "src");
    private static readonly string Tests = Path.Combine(RepositoryFiles.Root, "tests");

    private static bool InFolder(string path, string folder)
        => path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Contains(folder);

    private static List<string> Files(string root) => Directory
        .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
        .Where(path => !InFolder(path, "bin") && !InFolder(path, "obj"))
        .ToList();

    private static string Relative(string path) => Path.GetRelativePath(RepositoryFiles.Root, path).Replace('\\', '/');

    private static List<string> InventorySource() => Files(Source)
        .Where(path => !InFolder(path, "Migrations") && Relative(path).Contains("Inventory", StringComparison.Ordinal))
        .ToList();

    private static (string Up, string Down) Migration(string project, string file)
    {
        var text = File.ReadAllText(Path.Combine(Source, project, "Migrations", file));
        var up = text.IndexOf("void Up(", StringComparison.Ordinal);
        var down = text.IndexOf("void Down(", StringComparison.Ordinal);

        Assert.True(up > 0 && down > up);

        return (text[up..down], text[down..]);
    }

    [Fact]
    public void P2_X07_no_cross_repository_or_phase3_changes()
    {
        var inventory = InventorySource();
        string[] operational =
        [
            "Hold", "Confirm", "Release", "Expire", "ReservedQuantity", "Decrement", "SoldOut", "AvailableQuantity", "GuaranteedSellable", "StockPool", "ScopeKind",
            "ScopeReference", "ScopeValue", "HttpClient", FrozenAggregate, "AncillaryReservation", "JsonSerializer", "JsonDocument", "HasConversion"
        ];
        var offending = inventory
            .SelectMany(path => operational.Where(term => File.ReadAllText(path).Contains(term, StringComparison.Ordinal) || Path.GetFileName(path).Contains(term, StringComparison.Ordinal))
                .Select(term => $"{Relative(path)}: {term}"))
            .ToList();

        Assert.True(inventory.Count > 250);
        Assert.Empty(offending);

        var frozen = Files(Source)
            .Concat(Files(Tests))
            .Where(path => InFolder(path, FrozenAggregate)
                           || Path.GetFileName(path) is "M1ReservationAcceptanceTests.cs" or "M1ReservationConformanceTests.cs")
            .Select(path => (Path: Relative(path), Hash: Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path).Where(value => value != 13).ToArray()))))
            .OrderBy(file => file.Path, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(36, frozen.Count);
        Assert.Equal(
            FrozenSourceHash,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', frozen.Select(file => $"{file.Path}:{file.Hash}"))))));
        Assert.Contains("StockPoolId", File.ReadAllText(Files(Source).Single(path => Path.GetFileName(path) == "AncillaryReservationUnit.cs")), StringComparison.Ordinal);
        Assert.DoesNotContain(Files(Source).Where(path => !InFolder(path, FrozenAggregate) && !InFolder(path, "Migrations")), path => File.ReadAllText(path).Contains("StockPoolId", StringComparison.Ordinal));
    }

    [Fact]
    public void P2_R03_P05_only_the_four_approved_inventory_aggregates_exist_and_no_entitlement_or_usage_ledger_is_spoofed()
    {
        var aggregates = typeof(AncillaryInventoryPolicy).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(AggregateRoot<long>).IsAssignableFrom(type) && type.Name.Contains("Inventory", StringComparison.Ordinal))
            .Select(type => type.Name)
            .OrderBy(name => name, StringComparer.Ordinal);
        var repositories = typeof(AncillaryInventoryPolicy).Assembly.GetTypes()
            .Where(type => type.IsInterface && type.Name.EndsWith("Repository", StringComparison.Ordinal) && type.Name.Contains("Inventory", StringComparison.Ordinal))
            .Select(type => type.Name)
            .OrderBy(name => name, StringComparer.Ordinal);
        var names = Files(Source).Select(path => Path.GetFileNameWithoutExtension(path)!).ToList();

        Assert.Equal(new[] { "AirportSlotInventory", "AncillaryInventoryPolicy", "FlightCountInventory", "FlightWeightInventory" }, aggregates);
        Assert.Equal(new[] { "IAirportSlotInventoryRepository", "IFlightCountInventoryRepository", "IFlightWeightInventoryRepository", "IInventoryPolicyRepository" }, repositories);
        Assert.DoesNotContain(
            names,
            name => new[] { "DailyServiceInventory", "RoomNightInventory", "AssignedAsset", "StockPool", "InventoryResourceDefinition", "GenericInventory", "InventoryBase", "Entitlement", "UsageLedger" }
                .Any(term => name.Contains(term, StringComparison.Ordinal)));
        Assert.DoesNotContain(
            typeof(AncillaryInventoryPolicy).Assembly.GetTypes().Where(type => type.Name.Contains("Inventory", StringComparison.Ordinal) && type.IsClass),
            type => type.BaseType is { IsAbstract: true, Name: var name } && name.Contains("Inventory", StringComparison.Ordinal));
    }

    [Fact]
    public void P2_X06_the_stock_capacity_migrations_only_create_new_tables_and_indexes_and_drop_only_those_tables()
    {
        string[] commandTables =
        [
            "AirportSlotAdjustments", "AirportSlotInventories", "AncillaryInventoryPolicies", "FlightCountAdjustments", "FlightCountInventories", "FlightWeightAdjustments",
            "FlightWeightInventories", "InventoryPassengerUsageLimits"
        ];
        string[] readTables =
        [
            "AirportSlotAdjustments", "AirportSlotInventories", "AncillaryInventoryPassengerUsageLimits", "AncillaryInventoryPolicies", "FlightCountAdjustments",
            "FlightCountInventories", "FlightWeightAdjustments", "FlightWeightInventories"
        ];

        foreach (var (project, file, tables) in new[] { ("AeroTech.Ancillary.Persistence", CommandMigration, commandTables), ("AeroTech.Ancillary.Query", QueryMigration, readTables) })
        {
            var (up, down) = Migration(project, file);

            Assert.Equal(
                new[] { "CreateIndex", "CreateTable" },
                Regex.Matches(up, @"migrationBuilder\.([A-Za-z]+)").Select(match => match.Groups[1].Value).Distinct().OrderBy(name => name, StringComparer.Ordinal));
            Assert.Equal(tables, Regex.Matches(up, @"CreateTable\(\s*name: ""([A-Za-z]+)""").Select(match => match.Groups[1].Value).OrderBy(name => name, StringComparer.Ordinal));
            Assert.Equal(new[] { "DropTable" }, Regex.Matches(down, @"migrationBuilder\.([A-Za-z]+)").Select(match => match.Groups[1].Value).Distinct());
            Assert.Equal(tables, Regex.Matches(down, @"DropTable\(\s*name: ""([A-Za-z]+)""").Select(match => match.Groups[1].Value).OrderBy(name => name, StringComparer.Ordinal));
            Assert.DoesNotContain("Reservation", up, StringComparison.Ordinal);
            Assert.DoesNotContain("Provision", up, StringComparison.Ordinal);
            Assert.DoesNotContain("Pricing", up, StringComparison.Ordinal);
            Assert.DoesNotContain("migrationBuilder.Sql", up, StringComparison.Ordinal);
            Assert.Equal(4, Regex.Matches(up, @"type: ""decimal\(18,3\)""").Select(match => match.Index).Count());
        }

        var command = Migration("AeroTech.Ancillary.Persistence", CommandMigration).Up;

        Assert.Equal(
            new[]
            {
                "IX_AirportSlotInventories_PhysicalKey_Current", "IX_AncillaryInventoryPolicies_Identity_Current", "IX_FlightCountInventories_PhysicalKey_Current",
                "IX_FlightWeightInventories_PhysicalKey_Current"
            },
            Regex.Matches(command, @"name: ""(IX_[A-Za-z_]+)"",[^;]*?filter: ""\[Status\] <> 4""\)", RegexOptions.Singleline).Select(match => match.Groups[1].Value).OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal(4, Regex.Matches(command, @"rowVersion: true").Count);
        Assert.Equal(
            new[] { "AirportSlotInventories", "AncillaryInventoryPolicies", "AncillaryServiceDefinitions", "FlightCountInventories", "FlightWeightInventories" },
            Regex.Matches(command, @"principalTable: ""([A-Za-z]+)""").Select(match => match.Groups[1].Value).OrderBy(name => name, StringComparer.Ordinal));
    }
}
