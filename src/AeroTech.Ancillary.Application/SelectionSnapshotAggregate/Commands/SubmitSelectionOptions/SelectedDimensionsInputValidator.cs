using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions
{
    public sealed class SelectedDimensionsInputValidator : AbstractValidator<SelectedDimensionsInput>
    {
        public SelectedDimensionsInputValidator()
        {
            RuleFor(dimensions => dimensions.LengthCm).GreaterThan(0);
            RuleFor(dimensions => dimensions.WidthCm).GreaterThan(0);
            RuleFor(dimensions => dimensions.HeightCm).GreaterThan(0);
        }
    }
}
