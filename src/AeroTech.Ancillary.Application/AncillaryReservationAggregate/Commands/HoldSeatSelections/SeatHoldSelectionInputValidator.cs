using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections
{
    public sealed class SeatHoldSelectionInputValidator : AbstractValidator<SeatHoldSelectionInput>
    {
        public SeatHoldSelectionInputValidator()
        {
            RuleFor(seat => seat.OrderServiceId).GreaterThan(0);
            RuleFor(seat => seat.TravellerId).GreaterThan(0);
            RuleFor(seat => seat.SeatMapSelectionId).NotEmpty().MaximumLength(256);
            RuleFor(seat => seat.PassengerTypeCode).IsInEnum();
            RuleFor(seat => seat.Flight).NotNull().SetValidator(new SeatHoldFlightInputValidator());
            RuleFor(seat => seat.Fare!).SetValidator(new SeatHoldFareInputValidator()).When(seat => seat.Fare is not null);
        }
    }
}
