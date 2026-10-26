using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public sealed class AncillarySelectionLineValidator : AbstractValidator<AncillarySelectionLine>
    {
        public AncillarySelectionLineValidator()
        {
            RuleFor(line => line.OfferItemId).NotEmpty().MaximumLength(1024);
            RuleFor(line => line.Quantity).GreaterThan(0).When(line => line.Quantity is not null);
            RuleFor(line => line.Quantity)
                .Null()
                .When(line => line.Options is not null)
                .WithMessage("A quantity is not taken together with options.");
            RuleFor(line => line.Options)
                .Must(options => FamilyCount(options!) == 1)
                .When(line => line.Options is not null)
                .WithMessage("Exactly one of baggage, pet, assistedTravel, airportService, priority or connectivity must be set.");
            RuleFor(line => line.Options!)
                .SetValidator(new SelectionOptionsInputValidator())
                .When(line => line.Options is not null);
        }

        private static int FamilyCount(SelectionOptionsInput options)
            => new object?[] { options.Baggage, options.Pet, options.AssistedTravel, options.AirportService, options.Priority, options.Connectivity }
                .Count(family => family is not null);
    }
}
