using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public abstract class PriceAncillaryOffersValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IPriceAncillaryOffersCommand
    {
        protected PriceAncillaryOffersValidator()
        {
            RuleFor(command => command.OfferId).NotEmpty();
            RuleFor(command => command.Selections).NotEmpty();
            RuleFor(command => command.Selections.Count)
                .LessThanOrEqualTo(50)
                .When(command => command.Selections is not null);
            RuleForEach(command => command.Selections).NotNull().SetValidator(new AncillarySelectionLineValidator());
            RuleFor(command => command.Selections)
                .Must(selections => selections.Select(line => line?.OfferItemId).Distinct(StringComparer.Ordinal).Count() == selections.Count)
                .When(command => command.Selections is not null)
                .WithMessage("Each offerItemId must appear once.");
        }
    }
}
