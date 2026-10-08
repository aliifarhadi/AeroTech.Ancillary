using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
    public sealed class ProvisionPassengerCriteriaInputValidator : AbstractValidator<ProvisionPassengerCriteriaInput>
    {
        public ProvisionPassengerCriteriaInputValidator()
        {
            RuleForEach(passenger => passenger.PassengerTypeCodes).IsInEnum();
        }
    }

    public sealed class ProvisionSalesCriteriaInputValidator : AbstractValidator<ProvisionSalesCriteriaInput>
    {
        public ProvisionSalesCriteriaInputValidator()
        {
            RuleForEach(sales => sales.CustomerTypes).IsInEnum();
        }
    }

    public sealed class ProvisionTravelCriteriaInputValidator : AbstractValidator<ProvisionTravelCriteriaInput>
    {
        public ProvisionTravelCriteriaInputValidator()
        {
            RuleForEach(travel => travel.RoutePairs).NotNull().SetValidator(new ProvisionRoutePairInputValidator());
            RuleForEach(travel => travel.SeasonalPeriods).NotNull();
            RuleForEach(travel => travel.BlackoutPeriods).NotNull();
            RuleForEach(travel => travel.DayTimeRestrictions).NotNull().SetValidator(new ProvisionDayTimeRestrictionInputValidator());
            RuleForEach(travel => travel.FlightNumbers).NotEmpty().MaximumLength(16);
        }
    }

    public sealed class ProvisionRoutePairInputValidator : AbstractValidator<ProvisionRoutePairInput>
    {
        public ProvisionRoutePairInputValidator()
        {
            RuleFor(pair => pair.Direction).IsInEnum();
        }
    }

    public sealed class ProvisionDayTimeRestrictionInputValidator : AbstractValidator<ProvisionDayTimeRestrictionInput>
    {
        public ProvisionDayTimeRestrictionInputValidator()
        {
            RuleFor(restriction => restriction.DayOfWeek).IsInEnum();
            RuleFor(restriction => restriction.Effect).IsInEnum();
        }
    }

    public sealed class ProvisionFareCriteriaInputValidator : AbstractValidator<ProvisionFareCriteriaInput>
    {
        public ProvisionFareCriteriaInputValidator()
        {
            RuleForEach(fare => fare.AirFareTypes).IsInEnum();
            RuleForEach(fare => fare.FareBasisCodes).NotEmpty().MaximumLength(64);
        }
    }

    public sealed class ProvisionAdvancePurchaseInputValidator : AbstractValidator<ProvisionAdvancePurchaseInput>
    {
        public ProvisionAdvancePurchaseInputValidator()
        {
            RuleFor(advancePurchase => advancePurchase.Unit).IsInEnum();
        }
    }

    public sealed class ProvisionQuantityInputValidator : AbstractValidator<ProvisionQuantityInput>
    {
        public ProvisionQuantityInputValidator()
        {
            RuleFor(quantity => quantity.Unit).IsInEnum();
        }
    }

    public sealed class ProvisionApplicationInputValidator : AbstractValidator<ProvisionApplicationInput>
    {
        public ProvisionApplicationInputValidator()
        {
            RuleFor(application => application.Type).IsInEnum();
            RuleFor(application => application.Baggage)
                .SetValidator(new ProvisionBaggageApplicationInputValidator()!)
                .When(application => application.Baggage is not null);
            RuleFor(application => application.Seat)
                .SetValidator(new ProvisionSeatApplicationInputValidator()!)
                .When(application => application.Seat is not null);
        }
    }

    public sealed class ProvisionBaggageApplicationInputValidator : AbstractValidator<ProvisionBaggageApplicationInput>
    {
        public ProvisionBaggageApplicationInputValidator()
        {
            RuleFor(baggage => baggage.WeightUnit).IsInEnum();
            RuleFor(baggage => baggage.TravelApplication).IsInEnum().When(baggage => baggage.TravelApplication is not null);
            RuleFor(baggage => baggage.PurchaseApplication).IsInEnum();
            RuleFor(baggage => baggage.RuleDeference).IsInEnum().When(baggage => baggage.RuleDeference is not null);
        }
    }

    public sealed class ProvisionSeatApplicationInputValidator : AbstractValidator<ProvisionSeatApplicationInput>
    {
        public ProvisionSeatApplicationInputValidator()
        {
            RuleForEach(seat => seat.SeatNumbers).NotEmpty().MaximumLength(16);
            RuleForEach(seat => seat.SeatCharacteristicCodes).NotEmpty().MaximumLength(25);
        }
    }

    public sealed class ProvisionOutcomeInputValidator : AbstractValidator<ProvisionOutcomeInput>
    {
        public ProvisionOutcomeInputValidator()
        {
            RuleFor(outcome => outcome.Disposition).IsInEnum();
        }
    }

    public sealed class ProvisionSettlementInputValidator : AbstractValidator<ProvisionSettlementInput>
    {
        public ProvisionSettlementInputValidator()
        {
            RuleFor(settlement => settlement.ReissueRefund).IsInEnum();
            RuleFor(settlement => settlement.FormOfRefund).IsInEnum().When(settlement => settlement.FormOfRefund is not null);
        }
    }

    public sealed class ProvisionFulfillmentInputValidator : AbstractValidator<ProvisionFulfillmentInput>
    {
        public ProvisionFulfillmentInputValidator()
        {
            RuleFor(fulfillment => fulfillment.FulfillmentProviderKey).NotEmpty().MaximumLength(50);
        }
    }
}
