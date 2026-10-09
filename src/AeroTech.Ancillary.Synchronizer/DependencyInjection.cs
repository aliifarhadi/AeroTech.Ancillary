using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Synchronizer.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Synchronizer.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Synchronizer.AncillaryPricingAggregate;
using AeroTech.Ancillary.Synchronizer.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Synchronizer.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Synchronizer.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Synchronizer.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Synchronizer.SupplierAggregate;
using AeroTech.Ancillary.Synchronizer._Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Synchronizer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSynchronizer(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, AncillaryUnitOfWork>();
            services.AddScoped<ISupplierQueryDbSynchronizer, SupplierQueryDbSynchronizer>();
            services.AddScoped<IAncillaryServiceDefinitionQueryDbSynchronizer, AncillaryServiceDefinitionQueryDbSynchronizer>();
            services.AddScoped<IAncillaryProvisionQueryDbSynchronizer, AncillaryProvisionQueryDbSynchronizer>();
            services.AddScoped<IAncillaryPricingQueryDbSynchronizer, AncillaryPricingQueryDbSynchronizer>();
            services.AddScoped<IInventoryPolicyQueryDbSynchronizer, InventoryPolicyQueryDbSynchronizer>();
            services.AddScoped<IFlightCountInventoryQueryDbSynchronizer, FlightCountInventoryQueryDbSynchronizer>();
            services.AddScoped<IFlightWeightInventoryQueryDbSynchronizer, FlightWeightInventoryQueryDbSynchronizer>();
            services.AddScoped<IAirportSlotInventoryQueryDbSynchronizer, AirportSlotInventoryQueryDbSynchronizer>();

            return services;
        }
    }
}
