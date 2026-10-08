using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Framework.Infrastructure.Persistence;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
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

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

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
    }
}
