using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class SelectedUnitInputValidator : AbstractValidator<SelectedUnitInput>
    {
        public SelectedUnitInputValidator()
        {
            RuleFor(selected => selected.Quantity).GreaterThan(0).When(selected => selected.Quantity is not null);
        }
    }

    public sealed class SelectedUnitInputValidator<TOptions> : AbstractValidator<SelectedUnitInput<TOptions>>
        where TOptions : class
    {
        public SelectedUnitInputValidator(IValidator<TOptions> options)
        {
            RuleFor(selected => selected.Quantity).GreaterThan(0).When(selected => selected.Quantity is not null);
            RuleFor(selected => selected.Options!).SetValidator(options).When(selected => selected.Options is not null);
        }
    }
}
