using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct
{
    public sealed class ProductBaggageValidator : AbstractValidator<ProductBaggage>
    {
        public ProductBaggageValidator()
        {
            RuleFor(baggage => baggage.WeightUnit).IsInEnum();
        }
    }
}
