using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Models;
using AeroTech.Ancillary.Query.SupplierAggregate.Models;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.ReferenceData.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query._Shared.DbContexts
{
    public sealed class AncillaryQueryDbContext : DbContext
    {
        public const string ReadModelSchema = "ReadModel";
        public const string MigrationsHistorySchema = "dbo";
        public const string MigrationsHistoryTable = "__QueriesMigrationHistory";

        public AncillaryQueryDbContext(DbContextOptions<AncillaryQueryDbContext> options) : base(options)
        {
        }

        public DbSet<SupplierReadModel> Suppliers => Set<SupplierReadModel>();

        public DbSet<AncillaryServiceDefinitionReadModel> AncillaryServiceDefinitions => Set<AncillaryServiceDefinitionReadModel>();

        public DbSet<AncillaryProvisionReadModel> AncillaryProvisions => Set<AncillaryProvisionReadModel>();

        public DbSet<AncillaryProvisionPriceLineReadModel> AncillaryProvisionPriceLines => Set<AncillaryProvisionPriceLineReadModel>();

        public DbSet<CustomerReadModel> Customers => Set<CustomerReadModel>();

        public DbSet<CurrencyReadModel> Currencies => Set<CurrencyReadModel>();

        public DbSet<AirportReadModel> Airports => Set<AirportReadModel>();

        public DbSet<AirlineReadModel> Airlines => Set<AirlineReadModel>();

        public DbSet<CountryReadModel> Countries => Set<CountryReadModel>();

        public DbSet<AirlineOfficeReadModel> AirlineOffices => Set<AirlineOfficeReadModel>();

        public DbSet<TravelAgencyOfficeReadModel> TravelAgencyOffices => Set<TravelAgencyOfficeReadModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(ReadModelSchema);
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AncillaryQueryDbContext).Assembly);

            MapReferenceReadModel<CustomerReadModel>(modelBuilder, "Customers");
            MapReferenceReadModel<CurrencyReadModel>(modelBuilder, "Currencies");
            MapReferenceReadModel<AirportReadModel>(modelBuilder, "Airports");
            MapReferenceReadModel<AirlineReadModel>(modelBuilder, "Airlines");
            MapReferenceReadModel<CountryReadModel>(modelBuilder, "Countries");
            MapReferenceReadModel<AirlineOfficeReadModel>(modelBuilder, "AirlineOffices");
            MapReferenceReadModel<TravelAgencyOfficeReadModel>(modelBuilder, "TravelAgencyOffices");
        }

        private static void MapReferenceReadModel<TEntity>(ModelBuilder modelBuilder, string table)
            where TEntity : class
            => modelBuilder.Entity<TEntity>(entity =>
            {
                entity.ToTable(table, ReferenceDbContext.Schema, builder => builder.ExcludeFromMigrations());
                entity.HasKey("Id");
            });

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
            configurationBuilder.Properties<string>().HaveMaxLength(256);
        }
    }
}
