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
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeRestriction;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionSeasonalPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeRestriction;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeasonalPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeRestriction;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionSeasonalPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionTravelDate;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.RetireAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReviseAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.SuspendAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RetireSupplier;
using AeroTech.Ancillary.Application._Shared.Authorization;
using AeroTech.Ancillary.Application._Shared.Behaviors;
using AeroTech.Ancillary.Application._Shared.Events;
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
            services.AddScoped<IAddProvisionTravelDateService, AddProvisionTravelDateService>();
            services.AddScoped<IChangeProvisionTravelDateService, ChangeProvisionTravelDateService>();
            services.AddScoped<IRemoveProvisionTravelDateService, RemoveProvisionTravelDateService>();
            services.AddScoped<IAddProvisionSeasonalPeriodService, AddProvisionSeasonalPeriodService>();
            services.AddScoped<IChangeProvisionSeasonalPeriodService, ChangeProvisionSeasonalPeriodService>();
            services.AddScoped<IRemoveProvisionSeasonalPeriodService, RemoveProvisionSeasonalPeriodService>();
            services.AddScoped<IAddProvisionBlackoutPeriodService, AddProvisionBlackoutPeriodService>();
            services.AddScoped<IChangeProvisionBlackoutPeriodService, ChangeProvisionBlackoutPeriodService>();
            services.AddScoped<IRemoveProvisionBlackoutPeriodService, RemoveProvisionBlackoutPeriodService>();
            services.AddScoped<IAddProvisionDayTimeRestrictionService, AddProvisionDayTimeRestrictionService>();
            services.AddScoped<IChangeProvisionDayTimeRestrictionService, ChangeProvisionDayTimeRestrictionService>();
            services.AddScoped<IRemoveProvisionDayTimeRestrictionService, RemoveProvisionDayTimeRestrictionService>();
            services.AddScoped<IHoldAncillaryServicesService, HoldAncillaryServicesService>();
            services.AddScoped<IConfirmAncillaryHoldService, ConfirmAncillaryHoldService>();

            return services;
        }
    }
}
