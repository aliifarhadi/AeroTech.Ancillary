using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public abstract class PriceAncillaryOffersValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IPriceAncillaryOffersCommand
    {
        protected PriceAncillaryOffersValidator()
        {
            RuleFor(command => command.OfferId).NotEmpty();
            Include(new PriceSelectionsValidator<TCommand>());
        }
    }
}
