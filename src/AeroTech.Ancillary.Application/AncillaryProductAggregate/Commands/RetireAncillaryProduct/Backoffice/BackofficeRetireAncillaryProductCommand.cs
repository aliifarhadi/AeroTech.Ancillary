using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.RetireAncillaryProduct.Backoffice
{
    public sealed record BackofficeRetireAncillaryProductCommand(
        long AncillaryProductId) : IRequest<AncillaryProductResult>, IRetireAncillaryProductCommand;
}
