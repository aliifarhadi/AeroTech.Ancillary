using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections
{
    public abstract class HoldSeatSelectionsValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IHoldSeatSelectionsCommand
    {
        protected HoldSeatSelectionsValidator()
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(128);
            RuleFor(command => command.OrderId).GreaterThan(0);
            RuleFor(command => command.OrderCommercialVersion).GreaterThan(0);
            RuleFor(command => command.Reference).NotEmpty().MaximumLength(128);
            RuleFor(command => command.RequestedExpiresAt).NotEmpty();
            RuleFor(command => command.PointOfSaleId).GreaterThan(0);
            RuleFor(command => command.CurrencyId).GreaterThan(0);
            RuleFor(command => command.HoldId).GreaterThan(0).When(command => command.HoldId is not null);
            RuleFor(command => command.Seats).NotEmpty();
            RuleForEach(command => command.Seats).NotNull().SetValidator(new SeatHoldSelectionInputValidator());
            RuleForEach(command => command.Seats)
                .Must(seat => seat.CouponOpenForUse is not null)
                .When(command => command.TicketedAtUtc is not null)
                .WithName(nameof(SeatHoldSelectionInput.CouponOpenForUse));
        }
    }
}
