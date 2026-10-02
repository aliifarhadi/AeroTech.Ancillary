using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.SuspendAncillaryProduct
{
    public sealed class SuspendAncillaryProductService : ISuspendAncillaryProductService
    {
        private readonly IAncillaryProductRepository _products;
        private readonly IAncillaryProductQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public SuspendAncillaryProductService(
            IAncillaryProductRepository products,
            IAncillaryProductQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork)
        {
            _products = products;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<AncillaryProductResult> SuspendAsync(ISuspendAncillaryProductCommand command, CancellationToken cancellationToken = default)
        {
            var product = await _products.GetAsync(command.AncillaryProductId, cancellationToken)
                          ?? throw ExceptionFactory.AncillaryProductNotFound();

            product.Suspend();

            await _synchronizer.ProjectAsync(product.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AncillaryProductResult(product.Id, product.OwnerAirlineId, product.ProductRef, product.Version, product.Status);
        }
    }
}
