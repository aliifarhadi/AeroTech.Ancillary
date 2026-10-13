using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.PublishedReadModel;

[Collection(DatabaseCollection.Name)]
public class M1PublishedCatalogAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public M1PublishedCatalogAcceptanceTests(TestDatabase database) => _database = database;

    [Fact]
    public void M1_O01_O02_L05_no_issuance_activation_or_simulate_surface_exists()
    {
        var sources = Directory
            .EnumerateFiles(Path.Combine(RepositoryFiles.Root, "src", "AeroTech.Ancillary.RestApi"), "*.cs", SearchOption.AllDirectories)
            .Select(File.ReadAllText)
            .ToList();

        Assert.DoesNotContain(sources, source => source.Contains("Issuance", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(sources, source => source.Contains("Simulate", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void M1_A14_no_source_line_switches_on_a_supplier_id()
    {
        var offending = Directory
            .EnumerateFiles(Path.Combine(RepositoryFiles.Root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(File.ReadAllLines)
            .Where(line => line.Contains("switch", StringComparison.Ordinal) && line.Contains("SupplierId", StringComparison.Ordinal))
            .ToList();

        Assert.Empty(offending);
    }
}
