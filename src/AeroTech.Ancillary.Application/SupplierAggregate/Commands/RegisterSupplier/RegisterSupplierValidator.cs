using FluentValidation;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier
{
    public abstract class RegisterSupplierValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRegisterSupplierCommand
    {
        protected RegisterSupplierValidator()
        {
            RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
            RuleFor(command => command.FulfillmentKind).IsInEnum();
            RuleFor(command => command.FulfillmentProviderKey).MaximumLength(50);
        }
    }
}
