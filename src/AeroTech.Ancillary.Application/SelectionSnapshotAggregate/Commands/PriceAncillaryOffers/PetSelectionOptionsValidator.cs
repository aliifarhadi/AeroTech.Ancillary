using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public sealed class PetSelectionOptionsValidator : AbstractValidator<PetSelectionOptions>
    {
        public PetSelectionOptionsValidator()
        {
            RuleFor(options => options.CarrierDimensions!)
                .SetValidator(new SelectedDimensionsInputValidator())
                .When(options => options.CarrierDimensions is not null);
        }
    }
}
