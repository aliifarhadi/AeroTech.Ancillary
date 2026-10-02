using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ReviseAncillaryProduct.Backoffice
{
    public sealed record BackofficeReviseAncillaryProductCommand(
        long AncillaryProductId) : IRequest<AncillaryProductResult>, IReviseAncillaryProductCommand;
}
