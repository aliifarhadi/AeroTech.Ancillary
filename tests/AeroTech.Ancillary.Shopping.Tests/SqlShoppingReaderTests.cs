using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Reading.Sql;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
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
    public async Task SQL_01_the_sql_readers_feed_the_engine_with_the_active_typed_catalog_of_one_point_of_sale()
    {
        var airlineId = await PublishedCatalogAsync("A01", "A08", "A14", "A15", "A23", "A24");
        var draftVariant = V122Catalog.Case("A12");
        var supplier = await _proof.SupplierAsync(M1Commands.ExternalSupplier(airlineId, "OtherPartner"));

        await _proof.DefinitionAsync(V122Catalog.Define(draftVariant, airlineId, supplier), activate: false);

        await using var context = _database.NewContext(_clock);

        var references = new InMemoryCatalog();
        var trip = Trip.Context([Trip.Flight(departure: Summer)]) with { OwnerAirlineId = airlineId };
        var result = await Engine(context, references).ShopAsync(trip);

        Assert.Equal(new[] { "A01", "A08", "A14", "A15", "A23", "A24" }, result.Candidates.Select(candidate => candidate.VariantCode).Order());
        Assert.Empty(context.ChangeTracker.Entries());

        var bag = result.Candidates.Single(candidate => candidate.VariantCode == "A01");
        var seat = result.Candidates.Single(candidate => candidate.VariantCode == "A08");
        var pet = result.Candidates.Single(candidate => candidate.VariantCode == "A14");
        var wheelchair = result.Candidates.Single(candidate => candidate.VariantCode == "A15");

        Assert.Equal((OfferReadiness.Selectable, 100m, ShoppingLab.Eur), (bag.OfferReadiness, bag.Price.CompleteUnitTotal, bag.Price.CurrencyId));
        Assert.Contains(seat.Selection.Fields, field => field.Name == "ExitRowTermsAccepted");
        Assert.Contains(pet.Selection.Fields, field => field.Name == "SizeBracket");
        Assert.Equal((PriceOrigin.Free, OfferReadiness.NeedsSelection), (wheelchair.Price.Origin, wheelchair.OfferReadiness));

        var hold = await Engine(context, references).EvaluateSelectionAsync(
            trip,
            pet.CandidateIdentity,
            new AncillarySelection
            {
                AnimalType = PetAnimalType.Dog,
                CombinedWeightKg = 20m,
                CarrierDimensions = new SelectedDimensions(100m, 70m, 75m),
                DocumentAcknowledgements = ["HEALTH_CERT"],
                SizeBracket = "MEDIUM"
            });

        Assert.Equal(SelectionEvaluationStatus.Accepted, hold.Status);
        Assert.Empty((await Engine(context, references).ShopAsync(trip with { PointOfSaleId = V122Catalog.OtherPointOfSale })).Candidates);
        Assert.Empty((await Engine(context, references).ShopAsync(trip with { OwnerAirlineId = _database.NextAirlineId() })).Candidates);
    }

    [Fact]
    public async Task SQL_02_the_number_of_sql_commands_does_not_grow_with_travellers_flights_or_candidates()
    {
        var airlineId = await PublishedCatalogAsync("A01", "A12", "A23", "A24");

        await using var context = _database.NewContext(_clock);

        var engine = Engine(context, new InMemoryCatalog());
        var small = Trip.Context([Trip.Flight(departure: Summer)]) with { OwnerAirlineId = airlineId };
        var first = Trip.Flight("F1", 101, Trip.Thr, Trip.Ika, Summer);
        var second = Trip.Flight("F2", 102, Trip.Ika, Trip.Ist, first.DepartureAt.AddHours(5));
        var third = Trip.Flight("F3", 103, Trip.Ist, Trip.Thr, first.DepartureAt.AddDays(6), Trip.Istanbul);
        var large = Trip.Context(
            [first, second, third],
            [Trip.Portion("P1", 1, first, second), Trip.Portion("P2", 2, third)],
            [Trip.Adult("T1"), Trip.Adult("T2"), Trip.Adult("T3"), Trip.Child("T4")]) with { OwnerAirlineId = airlineId };

        async Task<(int Commands, int Candidates)> MeasureAsync(AncillaryShoppingContext trip)
        {
            var before = _database.Sql.Count;
            var result = await engine.ShopAsync(trip);

            return (_database.Sql.Count - before, result.Candidates.Count);
        }

        var one = await MeasureAsync(small);
        var many = await MeasureAsync(large);

        Assert.True(many.Candidates > one.Candidates * 3, $"{one.Candidates} -> {many.Candidates}");
        Assert.Equal(one.Commands, many.Commands);
        Assert.Empty(context.ChangeTracker.Entries());
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
            .AddSingleton<IFlightFlowDelegationReference>(references)
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
