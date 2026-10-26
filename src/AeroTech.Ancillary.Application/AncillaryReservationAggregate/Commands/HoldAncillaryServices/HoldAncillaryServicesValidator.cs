using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public abstract class HoldAncillaryServicesValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IHoldAncillaryServicesCommand
    {
        protected HoldAncillaryServicesValidator()
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(128);
            RuleFor(command => command.OrderId).GreaterThan(0);
            RuleFor(command => command.Reference).NotEmpty().MaximumLength(128);
            RuleFor(command => command.RequestedExpiresAt).NotEmpty();
            RuleForEach(command => command.Baggages).NotNull().SetValidator(Variants(new BaggageHoldOptionsInputValidator()));
            RuleForEach(command => command.Meals).NotNull().SetValidator(Variants());
            RuleForEach(command => command.AssistedTravel).NotNull().SetValidator(Variants(new AssistedTravelHoldOptionsInputValidator()));
            RuleForEach(command => command.AirportServices).NotNull().SetValidator(Variants(new AirportServiceHoldOptionsInputValidator()));
            RuleForEach(command => command.PriorityServices).NotNull().SetValidator(Variants(new PriorityHoldOptionsInputValidator()));
            RuleForEach(command => command.Connectivity).NotNull().SetValidator(Variants(new ConnectivityHoldOptionsInputValidator()));
            RuleForEach(command => command.Upgrades).NotNull().SetValidator(Variants());
            RuleFor(command => command)
                .Must(command => AncillaryHoldFamilies.OrderServiceIds(command).Count > 0)
                .WithName("units")
                .WithMessage("At least one unit must be held.");
            RuleFor(command => command)
                .Must(command => AncillaryHoldFamilies.OrderServiceIds(command) is var ids && ids.Distinct().Count() == ids.Count)
                .WithName(nameof(AncillaryUnitHoldInput<SelectedUnitInput>.OrderServiceId))
                .WithMessage("Each orderServiceId must appear once.");
        }

        private static AncillaryVariantHoldInputValidator<SelectedUnitInput<TOptions>> Variants<TOptions>(IValidator<TOptions> options)
            where TOptions : class
            => new(new SelectedUnitInputValidator<TOptions>(options));

        private static AncillaryVariantHoldInputValidator<SelectedUnitInput> Variants() => new(new SelectedUnitInputValidator());
    }
}
