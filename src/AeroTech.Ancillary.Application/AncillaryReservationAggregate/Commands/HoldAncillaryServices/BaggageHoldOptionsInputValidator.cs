using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class BaggageHoldOptionsInputValidator : AbstractValidator<BaggageHoldOptionsInput>
    {
        public BaggageHoldOptionsInputValidator()
        {
            RuleFor(options => options.Baggage).NotNull().SetValidator(new BaggageSelectionOptionsValidator());
        }
    }
}
