using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision
{
    public abstract class ChangeAncillaryProvisionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeAncillaryProvisionCommand
    {
        protected ChangeAncillaryProvisionValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.CoverageScope).IsInEnum();
            RuleFor(command => command.Quantity).NotNull().SetValidator(new ProvisionQuantityInputValidator());
            RuleFor(command => command.ApplicationType).IsInEnum();
            RuleFor(command => command.Outcome).NotNull().SetValidator(new ProvisionOutcomeInputValidator());
            RuleFor(command => command.Settlement).NotNull().SetValidator(new ProvisionSettlementInputValidator());
            RuleFor(command => command.Availability).NotNull();
            RuleFor(command => command.Fulfillment).NotNull().SetValidator(new ProvisionFulfillmentInputValidator());
            RuleFor(command => command.PassengerEligibility!)
                .SetValidator(new ProvisionPassengerEligibilityInputValidator())
                .When(command => command.PassengerEligibility is not null);
            RuleFor(command => command.SalesRestrictions!)
                .SetValidator(new ProvisionSalesRestrictionsInputValidator())
                .When(command => command.SalesRestrictions is not null);
            RuleFor(command => command.Geography!)
                .SetValidator(new ProvisionGeographyInputValidator())
                .When(command => command.Geography is not null);
            RuleFor(command => command.FlightApplication!)
                .SetValidator(new ProvisionFlightApplicationInputValidator())
                .When(command => command.FlightApplication is not null);
            RuleFor(command => command.FareApplication!)
                .SetValidator(new ProvisionFareApplicationInputValidator())
                .When(command => command.FareApplication is not null);
            RuleFor(command => command.TravelDate!)
                .SetValidator(new ProvisionTravelDateInputValidator())
                .When(command => command.TravelDate is not null);
            RuleFor(command => command.DayTimeApplication!)
                .SetValidator(new ProvisionDayTimeApplicationInputValidator())
                .When(command => command.DayTimeApplication is not null);
            RuleFor(command => command.AdvancePurchase!)
                .SetValidator(new ProvisionAdvancePurchaseInputValidator())
                .When(command => command.AdvancePurchase is not null);
            RuleFor(command => command.BaggageApplication!)
                .SetValidator(new ProvisionBaggageApplicationInputValidator())
                .When(command => command.BaggageApplication is not null);
            RuleFor(command => command.SeatApplication!)
                .SetValidator(new ProvisionSeatApplicationInputValidator())
                .When(command => command.SeatApplication is not null);
        }
    }
}
