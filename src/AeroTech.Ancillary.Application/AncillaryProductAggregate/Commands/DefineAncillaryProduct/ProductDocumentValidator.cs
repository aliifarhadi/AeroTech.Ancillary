using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct
{
    public sealed class ProductDocumentValidator : AbstractValidator<ProductDocument>
    {
        public ProductDocumentValidator()
        {
            RuleFor(document => document.Type).IsInEnum();
            RuleFor(document => document.Rfisc).Matches("^[A-Z0-9]{3}$");
        }
    }
}
