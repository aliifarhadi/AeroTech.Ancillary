using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ReviseAncillaryProduct
{
    public abstract class ReviseAncillaryProductValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IReviseAncillaryProductCommand
    {
        protected ReviseAncillaryProductValidator()
        {
            RuleFor(command => command.AncillaryProductId).GreaterThan(0);
        }
    }
}
