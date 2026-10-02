using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.RetireAncillaryProduct
{
    public abstract class RetireAncillaryProductValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRetireAncillaryProductCommand
    {
        protected RetireAncillaryProductValidator()
        {
            RuleFor(command => command.AncillaryProductId).GreaterThan(0);
        }
    }
}
