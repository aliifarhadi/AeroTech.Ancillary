using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetProvisionPricingsPaginated;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionForm;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionCreateForm;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionCreateForm;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionForm;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.VerifyGroupedSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.Orders;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.VerifySeatSelections;
using AeroTech.Framework.Infrastructure.HealthChecks;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoriesPaginated;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoryById;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryConfigurationSnapshot;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPoliciesPaginated;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyById;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyByServiceIdentity;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryVariants;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoriesPaginated;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoryById;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoriesPaginated;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoryById;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSupplierById;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Query
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddQuery(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("QueryDbContext")
                                   ?? configuration.GetConnectionString("CommandDbContext")
                                   ?? configuration.GetConnectionString("AncillaryDbContext");

            services.AddDbContext<AncillaryQueryDbContext>(options => options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsHistoryTable(AncillaryQueryDbContext.MigrationsHistoryTable, AncillaryQueryDbContext.MigrationsHistorySchema)));

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            services.AddScoped<IGetSupplierByIdService, GetSupplierByIdService>();
            services.AddScoped<IGetSuppliersPaginatedService, GetSuppliersPaginatedService>();
            services.AddScoped<IGetAncillaryServiceDefinitionByIdService, GetAncillaryServiceDefinitionByIdService>();
            services.AddScoped<IGetAncillaryServiceDefinitionFormService, GetAncillaryServiceDefinitionFormService>();
            services.AddScoped<IGetAncillaryServiceDefinitionsPaginatedService, GetAncillaryServiceDefinitionsPaginatedService>();
            services.AddScoped<IGetAncillaryVariantsService, GetAncillaryVariantsService>();
            services.AddSingleton(FlightOfferShoppingOptions.From(configuration));
            services.AddScoped<IFlightOfferShoppingSource, FlightOfferShoppingSource>();
            services.AddScoped<IOrderShoppingSource, OrderShoppingSource>();
            services.AddScoped<IShoppingSource, ShoppingSource>();
            services.AddScoped<IGetAncillaryOffersService, GetAncillaryOffersService>();
            services.AddScoped<IGetSeatMapService, GetSeatMapService>();
            services.AddScoped<IGetAncillaryOfferDetailService, GetAncillaryOfferDetailService>();
            services.AddScoped<IGetGroupedAncillaryOffersService, GetGroupedAncillaryOffersService>();
            services.AddScoped<IGroupedSelectionIdProvider, GroupedSelectionIdProvider>();
            services.AddScoped<IGetGroupedOfferDetailService, GetGroupedOfferDetailService>();
            services.AddScoped<GroupedSelectionRevalidation>();
            services.AddScoped<IResolveGroupedSelectionsService, ResolveGroupedSelectionsService>();
            services.AddScoped<IGroupedSelectionVerifier, GroupedSelectionVerifier>();
            services.AddScoped<IEvaluateSelectionService, EvaluateSelectionService>();
            services.AddSingleton(PricedSelectionOptions.From(configuration));
            services.AddScoped<IPricedSelectionTokens, PricedSelectionTokens>();
            services.AddScoped<IEvaluateBatchService, EvaluateBatchService>();
            services.AddScoped<IResolveSelectionsService, ResolveSelectionsService>();
            services.AddScoped<IResolveSeatMapSelectionsService, ResolveSeatMapSelectionsService>();
            services.AddScoped<ISeatSelectionVerifier, SeatSelectionVerifier>();
            services.AddScoped<IPointOfSaleEligibilityReference, PointOfSaleEligibilityService>();
            services.AddScoped<IAgencySalesScopeService, AgencySalesScopeService>();
            services.AddScoped<IBackofficeSalesScopeResolver, BackofficeSalesScopeResolver>();
            services.AddScoped<IApiSalesScopeResolver, ApiSalesScopeResolver>();
            services.AddScoped<IApiOfferReferencesReader, ApiOfferReferencesReader>();
            services.AddScoped<IOtaPanelSalesScopeResolver, OtaPanelSalesScopeResolver>();
            services.AddScoped<IGetAncillaryProvisionByIdService, GetAncillaryProvisionByIdService>();
            services.AddScoped<IGetAncillaryProvisionFormService, GetAncillaryProvisionFormService>();
            services.AddScoped<IGetSeatProvisionCreateFormService, GetSeatProvisionCreateFormService>();
            services.AddScoped<IGetSeatProvisionFormService, GetSeatProvisionFormService>();
            services.AddScoped<IGetProfileProvisionCreateFormService, GetProfileProvisionCreateFormService>();
            services.AddScoped<IGetProfileProvisionFormService, GetProfileProvisionFormService>();
            services.AddScoped<IGetAncillaryProvisionsPaginatedService, GetAncillaryProvisionsPaginatedService>();
            services.AddScoped<IGetAncillaryPricingByIdService, GetAncillaryPricingByIdService>();
            services.AddScoped<IGetAncillaryPricingsPaginatedService, GetAncillaryPricingsPaginatedService>();
            services.AddScoped<IGetProvisionPricingsPaginatedService, GetProvisionPricingsPaginatedService>();
            services.AddScoped<IGetAncillaryHoldByIdService, GetAncillaryHoldByIdService>();
            services.AddScoped<IGetInventoryPolicyByIdService, GetInventoryPolicyByIdService>();
            services.AddScoped<IGetInventoryPolicyByServiceIdentityService, GetInventoryPolicyByServiceIdentityService>();
            services.AddScoped<IGetInventoryPoliciesPaginatedService, GetInventoryPoliciesPaginatedService>();
            services.AddScoped<IGetInventoryConfigurationSnapshotService, GetInventoryConfigurationSnapshotService>();
            services.AddScoped<IGetFlightCountInventoryByIdService, GetFlightCountInventoryByIdService>();
            services.AddScoped<IGetFlightCountInventoriesPaginatedService, GetFlightCountInventoriesPaginatedService>();
            services.AddScoped<IGetFlightWeightInventoryByIdService, GetFlightWeightInventoryByIdService>();
            services.AddScoped<IGetFlightWeightInventoriesPaginatedService, GetFlightWeightInventoriesPaginatedService>();
            services.AddScoped<IGetAirportSlotInventoryByIdService, GetAirportSlotInventoryByIdService>();
            services.AddScoped<IGetAirportSlotInventoriesPaginatedService, GetAirportSlotInventoriesPaginatedService>();

            services.AddHealthChecks().AddDbContextReadinessCheck<AncillaryQueryDbContext>("sql-server-query");

            return services;
        }
    }
}
