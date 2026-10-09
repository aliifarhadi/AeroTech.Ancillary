using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.ReferenceData.ReadModels;
using AeroTech.Framework.Core.Domain.Events;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed class TestDatabase : IAsyncLifetime
{
    private const string ConnectionStringName = "CommandDbContext";
    private const string EnvironmentVariable = "ConnectionStrings__CommandDbContext";
    private const string SettingsFile = "appsettings.Test.json";

    private readonly string _connectionString = new SqlConnectionStringBuilder(ServerConnectionString())
    {
        InitialCatalog = $"DotAirAncillary_Tests_{Guid.NewGuid():N}"
    }.ConnectionString;

    private int _lastAirlineId = 1_000;

    public SequentialIdGenerator Ids { get; } = new();

    public SqlCommandCounter Sql { get; } = new();

    public int NextAirlineId() => Interlocked.Increment(ref _lastAirlineId);

    public AncillaryDbContext NewContext(IClock clock)
        => new(
            new DbContextOptionsBuilder<AncillaryDbContext>()
                .UseSqlServer(_connectionString, sql => sql.MigrationsHistoryTable(AncillaryDbContext.MigrationsHistoryTable, AncillaryDbContext.MigrationsHistorySchema))
                .AddInterceptors(Sql)
                .Options,
            new AnonymousActorResolver(),
            clock,
            new IgnoringDomainEventDispatcher());

    public AncillaryQueryDbContext NewQueryContext()
        => new(
            new DbContextOptionsBuilder<AncillaryQueryDbContext>()
                .UseSqlServer(_connectionString, sql => sql.MigrationsHistoryTable(AncillaryQueryDbContext.MigrationsHistoryTable, AncillaryQueryDbContext.MigrationsHistorySchema))
                .AddInterceptors(Sql)
                .Options);

    public ReferenceDbContext NewReferenceContext()
        => new(
            new DbContextOptionsBuilder<ReferenceDbContext>()
                .UseSqlServer(_connectionString, sql => sql.MigrationsHistoryTable(ReferenceDbContext.MigrationsHistoryTable, ReferenceDbContext.MigrationsHistorySchema))
                .Options);

    public Task InitializeAsync() => InitializeAsync(null, null);

    public async Task InitializeAsync(string? commandMigration, string? queryMigration)
    {
        await using var command = NewContext(new FixedClock());
        await using var query = NewQueryContext();
        await using var reference = NewReferenceContext();

        await command.GetService<IMigrator>().MigrateAsync(commandMigration);
        await query.GetService<IMigrator>().MigrateAsync(queryMigration);
        await reference.Database.MigrateAsync();

        reference.Currencies.AddRange(
            new CurrencyReadModel { Id = 47, Code = "EUR", DecimalPlaces = 2, RoundingFactor = 0.01 },
            new CurrencyReadModel { Id = 53, Code = "GBP", DecimalPlaces = 2, RoundingFactor = 0.01 },
            new CurrencyReadModel { Id = 70, Code = "IRR", DecimalPlaces = 0, RoundingFactor = 1 },
            new CurrencyReadModel { Id = 75, Code = "JPY", DecimalPlaces = 0, RoundingFactor = 1 },
            new CurrencyReadModel { Id = 82, Code = "KWD", DecimalPlaces = 3, RoundingFactor = 0.001 },
            new CurrencyReadModel { Id = 155, Code = "USD", DecimalPlaces = 2, RoundingFactor = 0.01 });
        await reference.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = NewContext(new FixedClock());

        await context.Database.EnsureDeletedAsync();
    }

    private static string ServerConnectionString()
        => Environment.GetEnvironmentVariable(EnvironmentVariable) is { Length: > 0 } fromEnvironment
            ? fromEnvironment
            : new ConfigurationBuilder()
                  .AddJsonFile(RepositoryFiles.TestProjectFile(SettingsFile), optional: true)
                  .Build()
                  .GetConnectionString(ConnectionStringName)
              ?? throw new InvalidOperationException(
                  $"The test database server is not configured: set ConnectionStrings:{ConnectionStringName} in {SettingsFile} of the acceptance test project or the environment variable {EnvironmentVariable}.");

    private sealed class AnonymousActorResolver : IActorResolver
    {
        public Actor Resolve() => Actor.Anonymous;
    }

    private sealed class IgnoringDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
