using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct
{
    public sealed class ProductQuantityValidator : AbstractValidator<ProductQuantity>
    {
        public ProductQuantityValidator()
        {
            RuleFor(quantity => quantity.Unit).IsInEnum();
        }
    }
}
