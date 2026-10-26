using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class AssistedTravelHoldOptionsInputValidator : AbstractValidator<AssistedTravelHoldOptionsInput>
    {
        public AssistedTravelHoldOptionsInputValidator()
        {
            RuleFor(options => options.AssistedTravel).NotNull();
        }
    }
}
