using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier;
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
            services.AddScoped<IDefineAncillaryServiceDefinitionService, DefineAncillaryServiceDefinitionService>();
            services.AddScoped<IActivateAncillaryServiceDefinitionService, ActivateAncillaryServiceDefinitionService>();
            services.AddScoped<IDefineAncillaryProvisionService, DefineAncillaryProvisionService>();
            services.AddScoped<IActivateAncillaryProvisionService, ActivateAncillaryProvisionService>();
            services.AddScoped<IHoldAncillaryServicesService, HoldAncillaryServicesService>();
            services.AddScoped<IConfirmAncillaryHoldService, ConfirmAncillaryHoldService>();

            return services;
        }
    }
}
