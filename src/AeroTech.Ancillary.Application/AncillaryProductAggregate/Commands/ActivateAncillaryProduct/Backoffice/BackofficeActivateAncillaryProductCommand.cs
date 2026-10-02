using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct.Backoffice
{
    public sealed record BackofficeActivateAncillaryProductCommand(
        long AncillaryProductId) : IRequest<ActivateAncillaryProductResult>, IActivateAncillaryProductCommand;
}
