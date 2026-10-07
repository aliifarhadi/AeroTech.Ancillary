using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class AncillaryHoldServiceInputValidator : AbstractValidator<AncillaryHoldServiceInput>
    {
        public AncillaryHoldServiceInputValidator()
        {
            RuleFor(service => service.OrderServiceId).GreaterThan(0);
            RuleFor(service => service.ServiceDefinitionId).GreaterThan(0);
            RuleFor(service => service.ProvisionId).GreaterThan(0);
            RuleFor(service => service.CoverageScope).IsInEnum();
            RuleFor(service => service.CoveredFlightIds).NotNull();
        }
    }
}
