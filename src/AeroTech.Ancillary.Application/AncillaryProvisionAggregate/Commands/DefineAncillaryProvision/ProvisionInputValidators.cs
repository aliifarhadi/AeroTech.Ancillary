using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
    public sealed class ProvisionPassengerEligibilityInputValidator : AbstractValidator<ProvisionPassengerEligibilityInput>
    {
        public ProvisionPassengerEligibilityInputValidator()
        {
            RuleForEach(passengerEligibility => passengerEligibility.AllowedPassengerTypes).IsInEnum();
            RuleForEach(passengerEligibility => passengerEligibility.AllowedAgeBands).NotNull();
        }
    }

    public sealed class ProvisionSalesRestrictionsInputValidator : AbstractValidator<ProvisionSalesRestrictionsInput>
    {
        public ProvisionSalesRestrictionsInputValidator()
        {
            RuleForEach(salesRestrictions => salesRestrictions.AllowedCustomerTypes).IsInEnum();
        }
    }

    public sealed class ProvisionGeographyInputValidator : AbstractValidator<ProvisionGeographyInput>
    {
        public ProvisionGeographyInputValidator()
        {
            RuleForEach(geography => geography.AllowedRoutePairs).NotNull().SetValidator(new ProvisionRoutePairInputValidator());
            RuleForEach(geography => geography.ServiceLocations).NotNull().SetValidator(new ProvisionServiceLocationInputValidator());
        }
    }

    public sealed class ProvisionRoutePairInputValidator : AbstractValidator<ProvisionRoutePairInput>
    {
        public ProvisionRoutePairInputValidator()
        {
            RuleFor(pair => pair.Direction).IsInEnum();
        }
    }

    public sealed class ProvisionServiceLocationInputValidator : AbstractValidator<ProvisionServiceLocationInput>
    {
        public ProvisionServiceLocationInputValidator()
        {
            RuleFor(location => location.LocationType).IsInEnum();
        }
    }

    public sealed class ProvisionFlightApplicationInputValidator : AbstractValidator<ProvisionFlightApplicationInput>
    {
        public ProvisionFlightApplicationInputValidator()
        {
            RuleForEach(flightApplication => flightApplication.AllowedFlightNumbers).NotEmpty().MaximumLength(16);
        }
    }

    public sealed class ProvisionFareApplicationInputValidator : AbstractValidator<ProvisionFareApplicationInput>
    {
        public ProvisionFareApplicationInputValidator()
        {
            RuleForEach(fareApplication => fareApplication.AllowedAirFareTypes).IsInEnum();
            RuleForEach(fareApplication => fareApplication.AllowedFareBasisCodes).NotEmpty().MaximumLength(64);
        }
    }

    public sealed class ProvisionTravelDateInputValidator : AbstractValidator<ProvisionTravelDateInput>
    {
        public ProvisionTravelDateInputValidator()
        {
            RuleForEach(travelDate => travelDate.PermittedPeriods).NotNull();
            RuleForEach(travelDate => travelDate.BlackoutPeriods).NotNull();
        }
    }

    public sealed class ProvisionDayTimeApplicationInputValidator : AbstractValidator<ProvisionDayTimeApplicationInput>
    {
        public ProvisionDayTimeApplicationInputValidator()
        {
            RuleForEach(dayTimeApplication => dayTimeApplication.Windows).NotNull().SetValidator(new ProvisionDayTimeWindowInputValidator());
        }
    }

    public sealed class ProvisionDayTimeWindowInputValidator : AbstractValidator<ProvisionDayTimeWindowInput>
    {
        public ProvisionDayTimeWindowInputValidator()
        {
            RuleFor(window => window.DaysOfWeekMask).InclusiveBetween((byte)1, (byte)127);
            RuleFor(window => window.Effect).IsInEnum();
        }
    }

    public sealed class ProvisionAdvancePurchaseInputValidator : AbstractValidator<ProvisionAdvancePurchaseInput>
    {
        public ProvisionAdvancePurchaseInputValidator()
        {
            RuleFor(advancePurchase => advancePurchase.MinimumPeriod).GreaterThanOrEqualTo(0);
            RuleFor(advancePurchase => advancePurchase.Unit).IsInEnum();
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
            RuleFor(baggage => baggage.ChargeKind).IsInEnum().When(baggage => baggage.ChargeKind is not null);
            RuleFor(baggage => baggage.AllowanceConcept).IsInEnum().When(baggage => baggage.AllowanceConcept is not null);
        }
    }

    public sealed class ProvisionSeatApplicationInputValidator : AbstractValidator<ProvisionSeatApplicationInput>
    {
        public ProvisionSeatApplicationInputValidator()
        {
            RuleForEach(seatApplication => seatApplication.SeatNumbers).NotEmpty().MaximumLength(16);
            RuleForEach(seatApplication => seatApplication.SeatCharacteristicCodes).NotEmpty().MaximumLength(25);
        }
    }

    public sealed class ProvisionPetRuleInputValidator : AbstractValidator<ProvisionPetRuleInput>
    {
        public ProvisionPetRuleInputValidator()
        {
            RuleFor(petRule => petRule.CountryExceptionCode).MaximumLength(10);
            RuleFor(petRule => petRule.AcceptanceMode).IsInEnum();
        }
    }

    public sealed class ProvisionAssistedTravelRuleInputValidator : AbstractValidator<ProvisionAssistedTravelRuleInput>
    {
        public ProvisionAssistedTravelRuleInputValidator()
        {
            RuleFor(assistedTravelRule => assistedTravelRule.ConnectionPolicy).IsInEnum().When(assistedTravelRule => assistedTravelRule.ConnectionPolicy is not null);
        }
    }

    public sealed class ProvisionAirportServiceRuleInputValidator : AbstractValidator<ProvisionAirportServiceRuleInput>
    {
        public ProvisionAirportServiceRuleInputValidator()
        {
            RuleFor(airportServiceRule => airportServiceRule.TerminalRef).MaximumLength(30);
            RuleFor(airportServiceRule => airportServiceRule.Direction).IsInEnum().When(airportServiceRule => airportServiceRule.Direction is not null);
        }
    }

    public sealed class ProvisionQuantityInputValidator : AbstractValidator<ProvisionQuantityInput>
    {
        public ProvisionQuantityInputValidator()
        {
            RuleFor(quantity => quantity.Unit).IsInEnum();
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
