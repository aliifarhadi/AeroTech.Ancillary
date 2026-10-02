using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Framework.Infrastructure.Persistence;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
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

        public DbSet<ServiceSubCode> ServiceSubCodes => Set<ServiceSubCode>();

        public DbSet<AncillaryProduct> AncillaryProducts => Set<AncillaryProduct>();

        public DbSet<AncillaryPriceRule> AncillaryPriceRules => Set<AncillaryPriceRule>();

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
