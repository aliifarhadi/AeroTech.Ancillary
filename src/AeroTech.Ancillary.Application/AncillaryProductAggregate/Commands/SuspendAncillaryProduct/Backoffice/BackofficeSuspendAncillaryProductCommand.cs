using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.SuspendAncillaryProduct.Backoffice
{
    public sealed record BackofficeSuspendAncillaryProductCommand(
        long AncillaryProductId) : IRequest<AncillaryProductResult>, ISuspendAncillaryProductCommand;
}
