using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct
{
    public abstract class ChangeAncillaryProductValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeAncillaryProductCommand
    {
        protected ChangeAncillaryProductValidator()
        {
            RuleFor(command => command.AncillaryProductId).GreaterThan(0);
            RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
            RuleFor(command => command.Description).MaximumLength(500);
            RuleFor(command => command.SalesScope).IsInEnum();
            RuleFor(command => command.Quantity).NotNull().SetValidator(new ProductQuantityValidator());
            RuleFor(command => command.Document).NotNull().SetValidator(new ProductDocumentValidator());
            RuleFor(command => command.Codes).NotNull().SetValidator(new ProductCodesValidator());
            RuleFor(command => command.Terms).NotNull().SetValidator(new ProductTermsValidator());
            RuleFor(command => command.InventoryControl).IsInEnum();
            RuleFor(command => command.Baggage!).SetValidator(new ProductBaggageValidator());
        }
    }
}
