using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct
{
    public abstract class ActivateAncillaryProductValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IActivateAncillaryProductCommand
    {
        protected ActivateAncillaryProductValidator()
        {
            RuleFor(command => command.AncillaryProductId).GreaterThan(0);
        }
    }
}
