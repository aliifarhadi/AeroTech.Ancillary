using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct
{
    public interface IChangeAncillaryProductService
    {
        Task<AncillaryProductResult> ChangeAsync(IChangeAncillaryProductCommand command, CancellationToken cancellationToken = default);
    }
}
