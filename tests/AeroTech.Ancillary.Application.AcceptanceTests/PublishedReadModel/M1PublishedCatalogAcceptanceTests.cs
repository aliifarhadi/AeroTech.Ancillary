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
    public async Task M1_U01_A06_the_published_rows_carry_what_the_airavail_catalog_reads()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var definition = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definition.Id));
        var provision = await scope.DefineProvision.DefineAsync(
            M1Commands.LoungeProvision(definition.Id, disposition: CommercialDisposition.Paid));
        var pricing = await scope.DefinePricing.DefineAsync(M1Commands.LoungePricing(provision.Id));
        await scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provision.Id, pricing.Id));

        await using var reader = new AncillaryScope(_database, _clock);

        var supplierRow = await reader.Query.Suppliers.AsNoTracking().SingleAsync(row => row.Id == supplier.Id);

        Assert.Equal(airlineId, supplierRow.OwnerAirlineId);
        Assert.Equal(SupplierStatus.Active, supplierRow.Status);
        Assert.Equal(_clock.Now, supplierRow.LastUpdateTime);

        var definitionRow = await reader.Query.AncillaryServiceDefinitions.AsNoTracking().SingleAsync(row => row.Id == definition.Id);

        Assert.Equal(supplier.Id, definitionRow.SupplierId);
        Assert.Equal("Dot Air", definitionRow.SupplierName);
        Assert.Equal("LNG_IKA_CIP", definitionRow.ServiceDefinitionRef);
        Assert.Equal(1, definitionRow.Version);
        Assert.Equal("0BX", definitionRow.ServiceSubCode);
        Assert.Equal(ServiceDefinitionStatus.Active, definitionRow.Status);
        Assert.Equal(_clock.Now, definitionRow.LastUpdateTime);

        var provisionRow = await reader.Query.AncillaryProvisions.AsNoTracking().SingleAsync(row => row.Id == provision.Id);

        Assert.Equal(definition.Id, provisionRow.ServiceDefinitionId);
        Assert.Equal(100, provisionRow.Sequence);
        Assert.Equal(ProvisionStatus.Active, provisionRow.Status);
        Assert.Equal(ServiceCoverageScope.Sector, provisionRow.CoverageScope);
        Assert.Equal(CommercialDisposition.Paid, provisionRow.Disposition);
        Assert.Equal("Ancillary", provisionRow.FulfillmentProviderKey);
        Assert.Equal(_clock.Now, provisionRow.LastUpdateTime);

        var pricingRow = await reader.Query.AncillaryPricings.AsNoTracking()
            .SingleAsync(row => row.AncillaryProvisionId == provision.Id && row.Status == PricingStatus.Active);

        Assert.Equal((pricing.Id, 1, PricingUnit.PerPassenger), (pricingRow.Id, pricingRow.Version, pricingRow.PricingUnit!.Value));
        Assert.Equal(M1Commands.Currency, pricingRow.CurrencyId);
        Assert.Equal(FeeApplicationUnit.Item, pricingRow.FeeApplicationUnit);
        Assert.Equal(_clock.Now, pricingRow.LastUpdateTime);

        var lineRow = await reader.Query.AncillaryPricingLines.AsNoTracking()
            .SingleAsync(row => row.AncillaryPricingId == pricingRow.Id);

        Assert.Equal(AncillaryPriceLineCategory.Ancillary, lineRow.Category);
        Assert.Equal(2500000m, lineRow.Amount);
        Assert.Equal(((int?)null, (int?)null, (int?)null), ((int?)lineRow.PassengerTypeCode, lineRow.AgeFromInclusive, lineRow.AgeToExclusive));
    }

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
