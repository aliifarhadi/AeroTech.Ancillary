using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public abstract class PriceOrderAncillaryOffersValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IPriceOrderAncillaryOffersCommand
    {
        protected PriceOrderAncillaryOffersValidator()
        {
            RuleFor(command => command.OrderId).NotNull().GreaterThan(0);
            Include(new PriceSelectionsValidator<TCommand>());
        }
    }
}
