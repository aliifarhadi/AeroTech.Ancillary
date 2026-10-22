using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections
{
    public sealed class SeatHoldFareInputValidator : AbstractValidator<SeatHoldFareInput>
    {
        public SeatHoldFareInputValidator()
        {
            RuleFor(fare => fare.AirFareId).GreaterThan(0);
            RuleFor(fare => fare.AirFareType).IsInEnum();
            RuleFor(fare => fare.FareBasis).MaximumLength(64);
            RuleFor(fare => fare.BookingClass).MaximumLength(2);
        }
    }
}
