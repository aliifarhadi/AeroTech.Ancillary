using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats
{
    public abstract class PriceOfferSeatsValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IPriceOfferSeatsCommand
    {
        protected PriceOfferSeatsValidator()
        {
            RuleFor(command => command.OfferId).NotEmpty();
            Include(new PriceSeatsValidator<TCommand>());
        }
    }
}
