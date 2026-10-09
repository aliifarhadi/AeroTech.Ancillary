using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Models;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Models;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Models;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Models;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Models;
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

        public DbSet<AncillaryProvisionPassengerTypeReadModel> AncillaryProvisionPassengerTypes => Set<AncillaryProvisionPassengerTypeReadModel>();

        public DbSet<AncillaryProvisionPointOfSaleReadModel> AncillaryProvisionPointsOfSale => Set<AncillaryProvisionPointOfSaleReadModel>();

        public DbSet<AncillaryProvisionCustomerReadModel> AncillaryProvisionCustomers => Set<AncillaryProvisionCustomerReadModel>();

        public DbSet<AncillaryProvisionCustomerTypeReadModel> AncillaryProvisionCustomerTypes => Set<AncillaryProvisionCustomerTypeReadModel>();

        public DbSet<AncillaryProvisionOriginAirportReadModel> AncillaryProvisionOriginAirports => Set<AncillaryProvisionOriginAirportReadModel>();

        public DbSet<AncillaryProvisionDestinationAirportReadModel> AncillaryProvisionDestinationAirports => Set<AncillaryProvisionDestinationAirportReadModel>();

        public DbSet<AncillaryProvisionViaAirportReadModel> AncillaryProvisionViaAirports => Set<AncillaryProvisionViaAirportReadModel>();

        public DbSet<AncillaryProvisionRoutePairReadModel> AncillaryProvisionRoutePairs => Set<AncillaryProvisionRoutePairReadModel>();

        public DbSet<AncillaryProvisionMarketingAirlineReadModel> AncillaryProvisionMarketingAirlines => Set<AncillaryProvisionMarketingAirlineReadModel>();

        public DbSet<AncillaryProvisionOperatingAirlineReadModel> AncillaryProvisionOperatingAirlines => Set<AncillaryProvisionOperatingAirlineReadModel>();

        public DbSet<AncillaryProvisionFlightNumberReadModel> AncillaryProvisionFlightNumbers => Set<AncillaryProvisionFlightNumberReadModel>();

        public DbSet<AncillaryProvisionFlightReadModel> AncillaryProvisionFlights => Set<AncillaryProvisionFlightReadModel>();

        public DbSet<AncillaryProvisionAircraftReadModel> AncillaryProvisionAircraft => Set<AncillaryProvisionAircraftReadModel>();

        public DbSet<AncillaryProvisionAirFareReadModel> AncillaryProvisionAirFares => Set<AncillaryProvisionAirFareReadModel>();

        public DbSet<AncillaryProvisionAirFareTypeReadModel> AncillaryProvisionAirFareTypes => Set<AncillaryProvisionAirFareTypeReadModel>();

        public DbSet<AncillaryProvisionFareFamilyReadModel> AncillaryProvisionFareFamilies => Set<AncillaryProvisionFareFamilyReadModel>();

        public DbSet<AncillaryProvisionFareBasisReadModel> AncillaryProvisionFareBases => Set<AncillaryProvisionFareBasisReadModel>();

        public DbSet<AncillaryProvisionCabinClassReadModel> AncillaryProvisionCabinClasses => Set<AncillaryProvisionCabinClassReadModel>();

        public DbSet<AncillaryProvisionRbdReadModel> AncillaryProvisionRbds => Set<AncillaryProvisionRbdReadModel>();

        public DbSet<AncillaryProvisionBlackoutPeriodReadModel> AncillaryProvisionBlackoutPeriods => Set<AncillaryProvisionBlackoutPeriodReadModel>();

        public DbSet<AncillaryProvisionEligibleAgeBandReadModel> AncillaryProvisionEligibleAgeBands => Set<AncillaryProvisionEligibleAgeBandReadModel>();

        public DbSet<AncillaryProvisionServiceLocationReadModel> AncillaryProvisionServiceLocations => Set<AncillaryProvisionServiceLocationReadModel>();

        public DbSet<AncillaryProvisionCoverageCountryReadModel> AncillaryProvisionCoverageCountries => Set<AncillaryProvisionCoverageCountryReadModel>();

        public DbSet<AncillaryProvisionPermittedTravelPeriodReadModel> AncillaryProvisionPermittedTravelPeriods => Set<AncillaryProvisionPermittedTravelPeriodReadModel>();

        public DbSet<AncillaryProvisionDayTimeWindowReadModel> AncillaryProvisionDayTimeWindows => Set<AncillaryProvisionDayTimeWindowReadModel>();

        public DbSet<AncillaryProvisionSeatNumberReadModel> AncillaryProvisionSeatNumbers => Set<AncillaryProvisionSeatNumberReadModel>();

        public DbSet<AncillaryProvisionSeatCharacteristicReadModel> AncillaryProvisionSeatCharacteristics => Set<AncillaryProvisionSeatCharacteristicReadModel>();

        public DbSet<AncillaryPricingReadModel> AncillaryPricings => Set<AncillaryPricingReadModel>();

        public DbSet<AncillaryPricingLineReadModel> AncillaryPricingLines => Set<AncillaryPricingLineReadModel>();

        public DbSet<InventoryPolicyReadModel> AncillaryInventoryPolicies => Set<InventoryPolicyReadModel>();

        public DbSet<InventoryPassengerUsageLimitReadModel> AncillaryInventoryPassengerUsageLimits => Set<InventoryPassengerUsageLimitReadModel>();

        public DbSet<FlightCountInventoryReadModel> FlightCountInventories => Set<FlightCountInventoryReadModel>();

        public DbSet<FlightCountAdjustmentReadModel> FlightCountAdjustments => Set<FlightCountAdjustmentReadModel>();

        public DbSet<FlightWeightInventoryReadModel> FlightWeightInventories => Set<FlightWeightInventoryReadModel>();

        public DbSet<FlightWeightAdjustmentReadModel> FlightWeightAdjustments => Set<FlightWeightAdjustmentReadModel>();

        public DbSet<AirportSlotInventoryReadModel> AirportSlotInventories => Set<AirportSlotInventoryReadModel>();

        public DbSet<AirportSlotAdjustmentReadModel> AirportSlotAdjustments => Set<AirportSlotAdjustmentReadModel>();

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
