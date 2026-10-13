using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Reading.Sql;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

[CollectionDefinition(Name)]
public sealed class ShoppingDatabaseCollection : ICollectionFixture<TestDatabase>
{
    public const string Name = "ShoppingDatabase";
}

[Collection(ShoppingDatabaseCollection.Name)]
public class SqlShoppingReaderTests
{
    private static readonly DateTimeOffset Summer = new(2027, 6, 1, 10, 0, 0, Trip.TehranOffset);

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public SqlShoppingReaderTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private static AncillaryShoppingEngine Engine(AncillaryDbContext context, InMemoryCatalog references)
        => new(
            new SqlActiveAncillaryDefinitionReader(context),
            new SqlActiveAncillaryProvisionReader(context),
            new SqlActiveAncillaryPricingReader(context),
            new SqlInventoryConfigurationReader(context),
            references,
            references,
            references,
            references);

    private async Task<int> PublishedCatalogAsync(params string[] codes)
    {
        var airlineId = _database.NextAirlineId();
        var local = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var external = await _proof.SupplierAsync(M1Commands.ExternalSupplier(airlineId, V122Catalog.QuoteProvider));

        foreach (var code in codes)
        {
            var variant = V122Catalog.Case(code);
            var definition = await _proof.DefinitionAsync(V122Catalog.Define(variant, airlineId, V122Catalog.NeedsExternalSupplier(variant) ? external : local));
            var rule = V122Catalog.Rule(variant, definition.Id);

            await _proof.RuleAsync(rule, variant.PriceOrigin == PriceOrigin.Filed ? id => V12Commands.Pricing(id, ShoppingLab.Eur, V12Commands.Base(100m)) : null);
        }

        return airlineId;
    }

    [Fact]
    public async Task SQL_03_the_module_registration_resolves_the_engine_with_the_sql_readers()
    {
        await using var context = _database.NewContext(_clock);

        var references = new InMemoryCatalog();
        var services = new ServiceCollection()
            .AddScoped(_ => context)
            .AddSingleton<ICurrencyReference>(references)
            .AddSingleton<IInventoryResourceReference>(references)
            .AddSingleton<IAirportFacilityReference>(references)
            .AddSingleton<ICountingFamilyReference>(references)
            .AddAncillaryShopping();

        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = provider.CreateAsyncScope();

        Assert.IsType<AncillaryShoppingEngine>(scope.ServiceProvider.GetRequiredService<IAncillaryShoppingEngine>());
        Assert.IsType<SqlActiveAncillaryDefinitionReader>(scope.ServiceProvider.GetRequiredService<IActiveAncillaryDefinitionReader>());
        Assert.IsType<SqlActiveAncillaryProvisionReader>(scope.ServiceProvider.GetRequiredService<IActiveAncillaryProvisionReader>());
        Assert.IsType<SqlActiveAncillaryPricingReader>(scope.ServiceProvider.GetRequiredService<IActiveAncillaryPricingReader>());
        Assert.IsType<SqlInventoryConfigurationReader>(scope.ServiceProvider.GetRequiredService<IInventoryConfigurationReader>());
    }
}
