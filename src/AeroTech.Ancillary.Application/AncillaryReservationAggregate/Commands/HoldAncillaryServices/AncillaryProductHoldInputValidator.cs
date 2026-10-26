using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class AncillaryProductHoldInputValidator<TSelected> : AbstractValidator<AncillaryProductHoldInput<TSelected>>
        where TSelected : class
    {
        public AncillaryProductHoldInputValidator(IValidator<TSelected> selected)
        {
            RuleFor(product => product.ProvisionId).GreaterThan(0);
            RuleFor(product => product.Units).NotEmpty();
            RuleForEach(product => product.Units).NotNull().SetValidator(new AncillaryUnitHoldInputValidator<TSelected>(selected));
        }
    }
}
