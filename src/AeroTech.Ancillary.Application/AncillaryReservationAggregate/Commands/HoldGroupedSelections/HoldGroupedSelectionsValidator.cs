using System.Linq.Expressions;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections
{
    public abstract class HoldGroupedSelectionsValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IHoldGroupedSelectionsCommand
    {
        protected HoldGroupedSelectionsValidator()
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(128);
            RuleFor(command => command.OrderId).GreaterThan(0);
            RuleFor(command => command.OrderCommercialVersion).GreaterThan(0);
            RuleFor(command => command.Reference).NotEmpty().MaximumLength(128);
            RuleFor(command => command.PointOfSaleId).GreaterThan(0);
            RuleFor(command => command.CurrencyId).GreaterThan(0);
            RuleFor(command => command.Seats).NotNull();
            RuleForEach(command => command.Seats).NotNull().SetValidator(new SeatHoldSelectionInputValidator());
            RuleForEach(command => command.Seats)
                .Must(seat => seat.CouponOpenForUse is not null)
                .When(command => command.TicketedAtUtc is not null)
                .WithName(nameof(SeatHoldSelectionInput.CouponOpenForUse));
            Selections(command => command.Baggages);
            Selections(command => command.Meals);
            Selections(command => command.Pets);
            Selections(command => command.AssistedTravel);
            Selections(command => command.AirportServices);
            Selections(command => command.PriorityServices);
            Selections(command => command.Connectivity);
            Selections(command => command.Upgrades);
            Selections(command => command.ExtraSeats);
        }

        private void Selections(Expression<Func<TCommand, IEnumerable<GroupedHoldSelectionInput>>> family)
        {
            RuleFor(family).NotNull();
            RuleForEach(family).NotNull().SetValidator(new GroupedHoldSelectionInputValidator());
        }
    }
}
