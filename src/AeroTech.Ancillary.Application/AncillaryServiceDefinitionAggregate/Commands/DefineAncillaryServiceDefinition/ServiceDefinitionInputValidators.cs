using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition
{
    public sealed class ServiceDefinitionDocumentInputValidator : AbstractValidator<ServiceDefinitionDocumentInput>
    {
        public ServiceDefinitionDocumentInputValidator()
        {
            RuleFor(document => document.Type).IsInEnum();
            RuleFor(document => document.Rfic).MaximumLength(1);
            RuleFor(document => document.Rfisc).MaximumLength(3);
        }
    }

    public sealed class ServiceDefinitionBookingInputValidator : AbstractValidator<ServiceDefinitionBookingInput>
    {
        public ServiceDefinitionBookingInputValidator()
        {
            RuleFor(booking => booking.Method).IsInEnum();
            RuleFor(booking => booking.SsrCode).MaximumLength(4);
            RuleFor(booking => booking.SsimCode).MaximumLength(4);
        }
    }
}
