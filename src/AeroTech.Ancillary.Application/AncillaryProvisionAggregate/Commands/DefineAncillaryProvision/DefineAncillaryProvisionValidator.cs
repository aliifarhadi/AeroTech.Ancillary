using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
    public abstract class DefineAncillaryProvisionValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IDefineAncillaryProvisionCommand
    {
        protected DefineAncillaryProvisionValidator()
        {
            RuleFor(command => command.CoverageScope).IsInEnum();
            RuleFor(command => command.Passenger)
                .SetValidator(new ProvisionPassengerCriteriaInputValidator()!)
                .When(command => command.Passenger is not null);
            RuleFor(command => command.Sales)
                .SetValidator(new ProvisionSalesCriteriaInputValidator()!)
                .When(command => command.Sales is not null);
            RuleFor(command => command.Travel)
                .SetValidator(new ProvisionTravelCriteriaInputValidator()!)
                .When(command => command.Travel is not null);
            RuleFor(command => command.Fare)
                .SetValidator(new ProvisionFareCriteriaInputValidator()!)
                .When(command => command.Fare is not null);
            RuleFor(command => command.AdvancePurchase)
                .SetValidator(new ProvisionAdvancePurchaseInputValidator()!)
                .When(command => command.AdvancePurchase is not null);
            RuleFor(command => command.Quantity).NotNull().SetValidator(new ProvisionQuantityInputValidator());
            RuleFor(command => command.Application).NotNull().SetValidator(new ProvisionApplicationInputValidator());
            RuleFor(command => command.Outcome).NotNull().SetValidator(new ProvisionOutcomeInputValidator());
            RuleFor(command => command.Fee).SetValidator(new ProvisionFeeInputValidator()!).When(command => command.Fee is not null);
            RuleFor(command => command.Settlement).NotNull().SetValidator(new ProvisionSettlementInputValidator());
            RuleFor(command => command.Availability).NotNull();
            RuleFor(command => command.Fulfillment).NotNull().SetValidator(new ProvisionFulfillmentInputValidator());
        }
    }
}
