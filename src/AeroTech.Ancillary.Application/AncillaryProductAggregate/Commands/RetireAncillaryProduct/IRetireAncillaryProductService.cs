using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.RetireAncillaryProduct
{
    public interface IRetireAncillaryProductService
    {
        Task<AncillaryProductResult> RetireAsync(IRetireAncillaryProductCommand command, CancellationToken cancellationToken = default);
    }
}
