using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ReviseAncillaryProduct
{
    public interface IReviseAncillaryProductService
    {
        Task<AncillaryProductResult> ReviseAsync(IReviseAncillaryProductCommand command, CancellationToken cancellationToken = default);
    }
}
