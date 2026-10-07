using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
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
        }
    }

    public sealed class ProvisionOutcomeInputValidator : AbstractValidator<ProvisionOutcomeInput>
    {
        public ProvisionOutcomeInputValidator()
        {
            RuleFor(outcome => outcome.Disposition).IsInEnum();
        }
    }

    public sealed class ProvisionFeeInputValidator : AbstractValidator<ProvisionFeeInput>
    {
        public ProvisionFeeInputValidator()
        {
            RuleFor(fee => fee.ApplicationUnit).IsInEnum();
            RuleFor(fee => fee.PriceLines).NotNull();
            RuleForEach(fee => fee.PriceLines).NotNull().SetValidator(new ProvisionPriceLineInputValidator());
        }
    }

    public sealed class ProvisionPriceLineInputValidator : AbstractValidator<ProvisionPriceLineInput>
    {
        public ProvisionPriceLineInputValidator()
        {
            RuleFor(line => line.Category).IsInEnum();
            RuleFor(line => line.Code).MaximumLength(10);
            RuleFor(line => line.Name).MaximumLength(100);
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
