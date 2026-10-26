using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class ConnectivityHoldOptionsInputValidator : AbstractValidator<ConnectivityHoldOptionsInput>
    {
        public ConnectivityHoldOptionsInputValidator()
        {
            RuleFor(options => options.Connectivity).NotNull();
        }
    }
}
