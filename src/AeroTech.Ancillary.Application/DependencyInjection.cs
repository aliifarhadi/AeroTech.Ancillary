using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ActivateAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReactivateAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.RetireAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReviseAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SuspendAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.CancelAncillaryHold;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ExpireAncillaryHolds;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.RetireAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReviseAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.SuspendAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RetireSupplier;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.ActivateAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.AdjustAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.CloseAirportSlotInventoryForSale;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.OpenAirportSlotInventoryForSale;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.RetireAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.SuspendAirportSlotInventory;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ActivateInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.RetireInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.SuspendInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Services;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.ActivateFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.CloseFlightCountInventoryForSale;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.OpenFlightCountInventoryForSale;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.RetireFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.SuspendFlightCountInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.ActivateFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.AdjustFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.CloseFlightWeightInventoryForSale;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.OpenFlightWeightInventoryForSale;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.RetireFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.SuspendFlightWeightInventory;
using AeroTech.Ancillary.Application._Shared.Authorization;
using AeroTech.Ancillary.Application._Shared.Behaviors;
using AeroTech.Ancillary.Application._Shared.Events;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            services.AddValidatorsFromAssembly(assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

            services.AddScoped<ICallerCustomer, CallerCustomer>();
            services.AddScoped<IDefineInventoryPolicyService, DefineInventoryPolicyService>();
            services.AddScoped<IChangeInventoryPolicyService, ChangeInventoryPolicyService>();
            services.AddScoped<IActivateInventoryPolicyService, ActivateInventoryPolicyService>();
            services.AddScoped<ISuspendInventoryPolicyService, SuspendInventoryPolicyService>();
            services.AddScoped<IRetireInventoryPolicyService, RetireInventoryPolicyService>();
            services.AddScoped<IDefineFlightCountInventoryService, DefineFlightCountInventoryService>();
            services.AddScoped<IActivateFlightCountInventoryService, ActivateFlightCountInventoryService>();
            services.AddScoped<IAdjustFlightCountInventoryService, AdjustFlightCountInventoryService>();
            services.AddScoped<ICloseFlightCountInventoryForSaleService, CloseFlightCountInventoryForSaleService>();
            services.AddScoped<IOpenFlightCountInventoryForSaleService, OpenFlightCountInventoryForSaleService>();
            services.AddScoped<ISuspendFlightCountInventoryService, SuspendFlightCountInventoryService>();
            services.AddScoped<IRetireFlightCountInventoryService, RetireFlightCountInventoryService>();
            services.AddScoped<IDefineFlightWeightInventoryService, DefineFlightWeightInventoryService>();
            services.AddScoped<IActivateFlightWeightInventoryService, ActivateFlightWeightInventoryService>();
            services.AddScoped<IAdjustFlightWeightInventoryService, AdjustFlightWeightInventoryService>();
            services.AddScoped<ICloseFlightWeightInventoryForSaleService, CloseFlightWeightInventoryForSaleService>();
            services.AddScoped<IOpenFlightWeightInventoryForSaleService, OpenFlightWeightInventoryForSaleService>();
            services.AddScoped<ISuspendFlightWeightInventoryService, SuspendFlightWeightInventoryService>();
            services.AddScoped<IRetireFlightWeightInventoryService, RetireFlightWeightInventoryService>();
            services.AddScoped<IDefineAirportSlotInventoryService, DefineAirportSlotInventoryService>();
            services.AddScoped<IActivateAirportSlotInventoryService, ActivateAirportSlotInventoryService>();
            services.AddScoped<IAdjustAirportSlotInventoryService, AdjustAirportSlotInventoryService>();
            services.AddScoped<ICloseAirportSlotInventoryForSaleService, CloseAirportSlotInventoryForSaleService>();
            services.AddScoped<IOpenAirportSlotInventoryForSaleService, OpenAirportSlotInventoryForSaleService>();
            services.AddScoped<ISuspendAirportSlotInventoryService, SuspendAirportSlotInventoryService>();
            services.AddScoped<IRetireAirportSlotInventoryService, RetireAirportSlotInventoryService>();
            services.AddScoped<IInventoryCallerScope, InventoryCallerScope>();
            services.AddScoped<IInventoryPolicyEvidenceBuilder, InventoryPolicyEvidenceBuilder>();
            services.AddScoped<IRegisterSupplierService, RegisterSupplierService>();
            services.AddScoped<IRetireSupplierService, RetireSupplierService>();
            services.AddScoped<IDefineAncillaryServiceDefinitionService, DefineAncillaryServiceDefinitionService>();
            services.AddScoped<IChangeAncillaryServiceDefinitionService, ChangeAncillaryServiceDefinitionService>();
            services.AddScoped<IActivateAncillaryServiceDefinitionService, ActivateAncillaryServiceDefinitionService>();
            services.AddScoped<ISuspendAncillaryServiceDefinitionService, SuspendAncillaryServiceDefinitionService>();
            services.AddScoped<IReactivateAncillaryServiceDefinitionService, ReactivateAncillaryServiceDefinitionService>();
            services.AddScoped<IRetireAncillaryServiceDefinitionService, RetireAncillaryServiceDefinitionService>();
            services.AddScoped<IReviseAncillaryServiceDefinitionService, ReviseAncillaryServiceDefinitionService>();
            services.AddScoped<IDefineAncillaryProvisionService, DefineAncillaryProvisionService>();
            services.AddScoped<IChangeAncillaryProvisionService, ChangeAncillaryProvisionService>();
            services.AddScoped<IActivateAncillaryProvisionService, ActivateAncillaryProvisionService>();
            services.AddScoped<ISuspendAncillaryProvisionService, SuspendAncillaryProvisionService>();
            services.AddScoped<IReactivateAncillaryProvisionService, ReactivateAncillaryProvisionService>();
            services.AddScoped<IRetireAncillaryProvisionService, RetireAncillaryProvisionService>();
            services.AddScoped<IAssignAncillaryServiceDefinitionPricingUnitService, AssignAncillaryServiceDefinitionPricingUnitService>();
            services.AddScoped<IDefineAncillaryPricingService, DefineAncillaryPricingService>();
            services.AddScoped<IChangeAncillaryPricingService, ChangeAncillaryPricingService>();
            services.AddScoped<IActivateAncillaryPricingService, ActivateAncillaryPricingService>();
            services.AddScoped<IReactivateAncillaryPricingService, ReactivateAncillaryPricingService>();
            services.AddScoped<ISuspendAncillaryPricingService, SuspendAncillaryPricingService>();
            services.AddScoped<IRetireAncillaryPricingService, RetireAncillaryPricingService>();
            services.AddScoped<IReviseAncillaryPricingService, ReviseAncillaryPricingService>();
            services.AddScoped<ISwitchActiveAncillaryPricingService, SwitchActiveAncillaryPricingService>();
            services.AddScoped<IPublishAncillaryProvisionService, PublishAncillaryProvisionService>();
            services.AddScoped<IAssignAncillaryServiceDefinitionServiceDateBasisService, AssignAncillaryServiceDefinitionServiceDateBasisService>();
            var holds = configuration.GetSection(AncillaryHoldOptions.SectionName).Get<AncillaryHoldOptions>();
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(holds?.LockExpirySeconds ?? 0, $"{AncillaryHoldOptions.SectionName}:{nameof(AncillaryHoldOptions.LockExpirySeconds)}");
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(holds?.ExpiryPollIntervalSeconds ?? 0, $"{AncillaryHoldOptions.SectionName}:{nameof(AncillaryHoldOptions.ExpiryPollIntervalSeconds)}");
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(holds?.ExpiryBatchSize ?? 0, $"{AncillaryHoldOptions.SectionName}:{nameof(AncillaryHoldOptions.ExpiryBatchSize)}");

            services.Configure<AncillaryHoldOptions>(configuration.GetSection(AncillaryHoldOptions.SectionName));
            services.AddScoped<IAncillaryHoldLock, AncillaryHoldLock>();
            services.AddScoped<IHoldAncillaryServicesService, HoldAncillaryServicesService>();
            services.AddScoped<IHoldSeatSelectionsService, HoldSeatSelectionsService>();
            services.AddScoped<IHoldGroupedSelectionsService, HoldGroupedSelectionsService>();
            services.AddScoped<IConfirmAncillaryHoldService, ConfirmAncillaryHoldService>();
            services.AddScoped<IReleaseAncillaryHoldService, ReleaseAncillaryHoldService>();
            services.AddScoped<ICancelAncillaryHoldService, CancelAncillaryHoldService>();
            services.AddScoped<IExpireAncillaryHoldsService, ExpireAncillaryHoldsService>();

            return services;
        }
    }
}
