using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition
{
    public abstract class ChangeAncillaryServiceDefinitionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeAncillaryServiceDefinitionCommand
    {
        protected ChangeAncillaryServiceDefinitionValidator()
        {
            RuleFor(command => command.ServiceDefinitionId).GreaterThan(0);
            RuleFor(command => command.ServiceSubCode).NotEmpty().MaximumLength(3);
            RuleFor(command => command.SubCodeSource).IsInEnum();
            RuleFor(command => command.ServiceTypeCode).MaximumLength(1);
            RuleFor(command => command.GroupCode).MaximumLength(2);
            RuleFor(command => command.SubGroupCode).MaximumLength(2);
            RuleFor(command => command.Description1Code).MaximumLength(2);
            RuleFor(command => command.Description2Code).MaximumLength(2);
            RuleFor(command => command.PricingUnit).IsInEnum();
            RuleFor(command => command.ServiceDateBasis).IsInEnum();
            RuleFor(command => command.Profile).IsInEnum();
            RuleFor(command => command.VariantCode).NotEmpty().MaximumLength(8);
            RuleFor(command => command.DocumentRouting).IsInEnum();
            RuleFor(command => command.Specification).NotNull().SetValidator(new ServiceSpecificationInputValidator());
            RuleFor(command => command.CommercialName).NotEmpty().MaximumLength(100);
            RuleFor(command => command.Description).MaximumLength(500);
            RuleFor(command => command.Document).NotNull().SetValidator(new ServiceDefinitionDocumentInputValidator());
            RuleFor(command => command.Booking).NotNull().SetValidator(new ServiceDefinitionBookingInputValidator());
        }
    }
}
