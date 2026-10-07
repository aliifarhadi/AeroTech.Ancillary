using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public abstract class HoldAncillaryServicesValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IHoldAncillaryServicesCommand
    {
        protected HoldAncillaryServicesValidator()
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(128);
            RuleFor(command => command.OrderId).GreaterThan(0);
            RuleFor(command => command.Reference).NotEmpty().MaximumLength(128);
            RuleFor(command => command.Services).NotEmpty();
            RuleForEach(command => command.Services).NotNull().SetValidator(new AncillaryHoldServiceInputValidator());
        }
    }
}
