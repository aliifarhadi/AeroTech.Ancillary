using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class PriorityHoldOptionsInputValidator : AbstractValidator<PriorityHoldOptionsInput>
    {
        public PriorityHoldOptionsInputValidator()
        {
            RuleFor(options => options.Priority).NotNull();
        }
    }
}
