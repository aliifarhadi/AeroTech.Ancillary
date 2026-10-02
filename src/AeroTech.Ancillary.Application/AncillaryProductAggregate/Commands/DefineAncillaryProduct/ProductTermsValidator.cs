using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct
{
    public sealed class ProductTermsValidator : AbstractValidator<ProductTerms>
    {
        public ProductTermsValidator()
        {
            RuleFor(terms => terms.FormOfRefundCode).MaximumLength(10);
        }
    }
}
