using FluentValidation;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RetireSupplier
{
    public abstract class RetireSupplierValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRetireSupplierCommand
    {
        protected RetireSupplierValidator()
        {
            RuleFor(command => command.SupplierId).GreaterThan(0);
        }
    }
}
