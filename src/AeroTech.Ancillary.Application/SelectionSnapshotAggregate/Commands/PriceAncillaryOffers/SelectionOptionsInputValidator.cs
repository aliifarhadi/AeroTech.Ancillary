using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public sealed class SelectionOptionsInputValidator : AbstractValidator<SelectionOptionsInput>
    {
        public SelectionOptionsInputValidator()
        {
            RuleFor(options => options.Baggage!)
                .SetValidator(new BaggageSelectionOptionsValidator())
                .When(options => options.Baggage is not null);
            RuleFor(options => options.Pet!)
                .SetValidator(new PetSelectionOptionsValidator())
                .When(options => options.Pet is not null);
        }
    }
}
