using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.Domain.Events;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

    public int NextAirlineId() => Interlocked.Increment(ref _lastAirlineId);

    public AncillaryDbContext NewContext(IClock clock)
        => new(
            new DbContextOptionsBuilder<AncillaryDbContext>()
                .UseSqlServer(_connectionString, sql => sql.MigrationsHistoryTable(AncillaryDbContext.MigrationsHistoryTable, AncillaryDbContext.MigrationsHistorySchema))
                .Options,
            new AnonymousActorResolver(),
            clock,
            new IgnoringDomainEventDispatcher());

    public AncillaryQueryDbContext NewQueryContext() => NewQueryContext(_connectionString);

    public AncillaryQueryDbContext NewScratchQueryContext(string name)
        => NewQueryContext(new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = $"{new SqlConnectionStringBuilder(_connectionString).InitialCatalog}_{name}"
        }.ConnectionString);

    public async Task InitializeAsync()
    {
        await using var command = NewContext(new FixedClock());
        await using var query = NewQueryContext();

        await command.Database.MigrateAsync();
        await query.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = NewContext(new FixedClock());

        await context.Database.EnsureDeletedAsync();
    }

    private static AncillaryQueryDbContext NewQueryContext(string connectionString)
        => new(
            new DbContextOptionsBuilder<AncillaryQueryDbContext>()
                .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable(AncillaryQueryDbContext.MigrationsHistoryTable, AncillaryQueryDbContext.MigrationsHistorySchema))
                .Options);

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
