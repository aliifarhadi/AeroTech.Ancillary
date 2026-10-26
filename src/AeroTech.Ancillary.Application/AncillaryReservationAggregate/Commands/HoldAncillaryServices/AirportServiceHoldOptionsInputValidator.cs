using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class AirportServiceHoldOptionsInputValidator : AbstractValidator<AirportServiceHoldOptionsInput>
    {
        public AirportServiceHoldOptionsInputValidator()
        {
            RuleFor(options => options.AirportService).NotNull();
        }
    }
}
