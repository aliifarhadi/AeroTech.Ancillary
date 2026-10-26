using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class AncillaryVariantHoldInputValidator<TSelected> : AbstractValidator<AncillaryVariantHoldInput<TSelected>>
        where TSelected : class
    {
        public AncillaryVariantHoldInputValidator(IValidator<TSelected> selected)
        {
            RuleFor(variant => variant.VariantCode).NotEmpty().MaximumLength(8);
            RuleFor(variant => variant.Products).NotEmpty();
            RuleForEach(variant => variant.Products).NotNull().SetValidator(new AncillaryProductHoldInputValidator<TSelected>(selected));
        }
    }
}
