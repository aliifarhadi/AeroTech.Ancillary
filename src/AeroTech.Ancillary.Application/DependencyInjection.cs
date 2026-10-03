using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ActivateAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ChangeAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.RetireAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.SuspendAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.RetireAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ReviseAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.SuspendAncillaryProduct;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.CancelServiceReservationUnits;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ConfirmServiceReservation;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReleaseServiceReservation;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.ReactivateServiceSubCode;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RetireServiceSubCode;
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
            services.AddScoped<IRegisterServiceSubCodeService, RegisterServiceSubCodeService>();
            services.AddScoped<IRetireServiceSubCodeService, RetireServiceSubCodeService>();
            services.AddScoped<IReactivateServiceSubCodeService, ReactivateServiceSubCodeService>();
            services.AddScoped<IDefineAncillaryProductService, DefineAncillaryProductService>();
            services.AddScoped<IChangeAncillaryProductService, ChangeAncillaryProductService>();
            services.AddScoped<IActivateAncillaryProductService, ActivateAncillaryProductService>();
            services.AddScoped<ISuspendAncillaryProductService, SuspendAncillaryProductService>();
            services.AddScoped<IRetireAncillaryProductService, RetireAncillaryProductService>();
            services.AddScoped<IReviseAncillaryProductService, ReviseAncillaryProductService>();
            services.AddScoped<IDefineAncillaryPriceRuleService, DefineAncillaryPriceRuleService>();
            services.AddScoped<IChangeAncillaryPriceRuleService, ChangeAncillaryPriceRuleService>();
            services.AddScoped<IActivateAncillaryPriceRuleService, ActivateAncillaryPriceRuleService>();
            services.AddScoped<ISuspendAncillaryPriceRuleService, SuspendAncillaryPriceRuleService>();
            services.AddScoped<IRetireAncillaryPriceRuleService, RetireAncillaryPriceRuleService>();
            services.AddScoped<IReserveServiceReservationService, ReserveServiceReservationService>();
            services.AddScoped<IConfirmServiceReservationService, ConfirmServiceReservationService>();
            services.AddScoped<IReleaseServiceReservationService, ReleaseServiceReservationService>();
            services.AddScoped<ICancelServiceReservationUnitsService, CancelServiceReservationUnitsService>();

            return services;
        }
    }
}
