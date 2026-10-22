using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections
{
    public sealed class GroupedHoldSelectionInputValidator : AbstractValidator<GroupedHoldSelectionInput>
    {
        public GroupedHoldSelectionInputValidator()
        {
            RuleFor(selection => selection.OrderServiceId).GreaterThan(0);
            RuleFor(selection => selection.SelectionId).NotEmpty().MaximumLength(4096);
        }
    }
}
