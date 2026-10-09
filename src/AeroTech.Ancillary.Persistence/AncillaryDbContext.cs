using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Framework.Infrastructure.Persistence;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Ancillary.Persistence.Inbox;
using AeroTech.Ancillary.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence
{
    public sealed class AncillaryDbContext : CommandDbContext, IUnitOfWork
    {
        public const string MigrationsHistorySchema = "dbo";
        public const string MigrationsHistoryTable = "__CommandsMigrationHistory";

        private static readonly string ProvisionRuleNamespace = typeof(ProvisionTravelDateRule).Namespace!;

        public AncillaryDbContext(
            DbContextOptions<AncillaryDbContext> options,
            IActorResolver actorResolver,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
            : base(options, actorResolver, clock, domainEventDispatcher)
        {
        }

        public DbSet<Supplier> Suppliers => Set<Supplier>();

        public DbSet<AncillaryServiceDefinition> AncillaryServiceDefinitions => Set<AncillaryServiceDefinition>();

        public DbSet<AncillaryProvision> AncillaryProvisions => Set<AncillaryProvision>();

        public DbSet<AncillaryPricing> AncillaryPricings => Set<AncillaryPricing>();

        public DbSet<AncillaryReservation> AncillaryReservations => Set<AncillaryReservation>();

        public DbSet<AncillaryInventoryPolicy> AncillaryInventoryPolicies => Set<AncillaryInventoryPolicy>();

        public DbSet<FlightCountInventory> FlightCountInventories => Set<FlightCountInventory>();

        public DbSet<FlightWeightInventory> FlightWeightInventories => Set<FlightWeightInventory>();

        public DbSet<AirportSlotInventory> AirportSlotInventories => Set<AirportSlotInventory>();

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            VersionProvisionsOfChangedRules();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            VersionProvisionsOfChangedRules();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Ancillary");
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AncillaryDbContext).Assembly);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
            configurationBuilder.Properties<string>().HaveMaxLength(256);
        }

        private void VersionProvisionsOfChangedRules()
        {
            var provisionIds = ChangeTracker.Entries()
                .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Where(entry => entry.Metadata.ClrType.Namespace == ProvisionRuleNamespace)
                .Select(entry => (long)entry.Property(nameof(ProvisionTravelDateRule.AncillaryProvisionId)).CurrentValue!)
                .ToHashSet();

            if (provisionIds.Count == 0)
                return;

            foreach (var provision in ChangeTracker.Entries<AncillaryProvision>())
            {
                if (provision.State == EntityState.Unchanged && provisionIds.Contains(provision.Entity.Id))
                    provision.State = EntityState.Modified;
            }
        }
    }
}
