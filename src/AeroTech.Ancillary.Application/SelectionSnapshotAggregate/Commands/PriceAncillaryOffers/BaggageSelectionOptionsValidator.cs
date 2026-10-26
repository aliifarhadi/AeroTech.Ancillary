using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public sealed class BaggageSelectionOptionsValidator : AbstractValidator<BaggageSelectionOptions>
    {
        public BaggageSelectionOptionsValidator()
        {
            RuleFor(options => options.Dimensions!)
                .SetValidator(new SelectedDimensionsInputValidator())
                .When(options => options.Dimensions is not null);
            RuleFor(options => options.WeightKg)
                .GreaterThan(0)
                .When(options => options.WeightKg is not null);
        }
    }
}
