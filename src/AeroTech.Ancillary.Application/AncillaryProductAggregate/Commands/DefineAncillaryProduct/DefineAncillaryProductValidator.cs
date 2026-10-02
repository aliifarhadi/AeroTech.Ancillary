using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct
{
    public abstract class DefineAncillaryProductValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IDefineAncillaryProductCommand
    {
        protected DefineAncillaryProductValidator()
        {
            RuleFor(command => command.ProductRef).NotEmpty().Matches("^[A-Z0-9]{2,20}$");
            RuleFor(command => command.Type).IsInEnum();
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
