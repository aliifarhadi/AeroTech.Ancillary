using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions
{
    public abstract class SubmitSelectionOptionsValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : ISubmitSelectionOptionsCommand
    {
        protected SubmitSelectionOptionsValidator()
        {
            RuleFor(command => command.Reference).NotEmpty().MaximumLength(1024);
            RuleFor(command => command.OfferId).NotEmpty();
            RuleFor(command => command.Options).NotNull();
            RuleFor(command => command.Options.Dimensions!)
                .SetValidator(new SelectedDimensionsInputValidator())
                .When(command => command.Options?.Dimensions is not null);
            RuleFor(command => command.Options.CarrierDimensions!)
                .SetValidator(new SelectedDimensionsInputValidator())
                .When(command => command.Options?.CarrierDimensions is not null);
            RuleFor(command => command.Options.WeightKg)
                .GreaterThan(0)
                .When(command => command.Options?.WeightKg is not null);
        }
    }
}
