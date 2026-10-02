using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct
{
    public sealed class ProductLoungeValidator : AbstractValidator<ProductLounge>
    {
        public ProductLoungeValidator()
        {
            RuleFor(lounge => lounge.AirportIds).NotNull();
        }
    }
}
