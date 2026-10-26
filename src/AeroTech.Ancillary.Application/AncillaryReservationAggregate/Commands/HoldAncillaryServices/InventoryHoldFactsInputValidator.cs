using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class InventoryHoldFactsInputValidator : AbstractValidator<InventoryHoldFactsInput>
    {
        public InventoryHoldFactsInputValidator()
        {
            RuleFor(facts => facts.TimeWithOffset).NotNull();
        }
    }
}
