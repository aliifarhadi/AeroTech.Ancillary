using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.RetireAncillaryProduct
{
    public sealed class RetireAncillaryProductService : IRetireAncillaryProductService
    {
        private readonly IAncillaryProductRepository _products;
        private readonly IAncillaryProductQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public RetireAncillaryProductService(
            IAncillaryProductRepository products,
            IAncillaryProductQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _products = products;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<AncillaryProductResult> RetireAsync(IRetireAncillaryProductCommand command, CancellationToken cancellationToken = default)
        {
            var product = await _products.GetAsync(command.AncillaryProductId, cancellationToken)
                          ?? throw ExceptionFactory.AncillaryProductNotFound();

            product.Retire(_clock.GetDateTime());

            await _synchronizer.ProjectAsync(product.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AncillaryProductResult(product.Id, product.OwnerAirlineId, product.ProductRef, product.Version, product.Status);
        }
    }
}
