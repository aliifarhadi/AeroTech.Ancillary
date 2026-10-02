using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct
{
    public sealed class ProductCodesValidator : AbstractValidator<ProductCodes>
    {
        public ProductCodesValidator()
        {
            RuleFor(codes => codes.ServiceTypeCode).NotEmpty().Matches("^[A-Z]$");
        }
    }
}
