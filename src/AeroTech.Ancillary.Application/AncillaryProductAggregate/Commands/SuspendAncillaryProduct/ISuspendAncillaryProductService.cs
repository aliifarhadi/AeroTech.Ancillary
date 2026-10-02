using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.SuspendAncillaryProduct
{
    public interface ISuspendAncillaryProductService
    {
        Task<AncillaryProductResult> SuspendAsync(ISuspendAncillaryProductCommand command, CancellationToken cancellationToken = default);
    }
}
