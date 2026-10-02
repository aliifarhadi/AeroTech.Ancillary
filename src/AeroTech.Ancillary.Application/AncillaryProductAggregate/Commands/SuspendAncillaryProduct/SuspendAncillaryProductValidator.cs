using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.SuspendAncillaryProduct
{
    public abstract class SuspendAncillaryProductValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ISuspendAncillaryProductCommand
    {
        protected SuspendAncillaryProductValidator()
        {
            RuleFor(command => command.AncillaryProductId).GreaterThan(0);
        }
    }
}
