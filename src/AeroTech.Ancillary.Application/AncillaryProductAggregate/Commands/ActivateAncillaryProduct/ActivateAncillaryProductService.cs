using AeroTech.Ancillary.Application.AncillaryProductAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct
{
    public sealed class ActivateAncillaryProductService : IActivateAncillaryProductService
    {
        private readonly IAncillaryProductRepository _products;
        private readonly IServiceSubCodeRepository _subCodes;
        private readonly IAncillaryProductQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public ActivateAncillaryProductService(
            IAncillaryProductRepository products,
            IServiceSubCodeRepository subCodes,
            IAncillaryProductQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _products = products;
            _subCodes = subCodes;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<ActivateAncillaryProductResult> ActivateAsync(IActivateAncillaryProductCommand command, CancellationToken cancellationToken = default)
        {
            var product = await _products.GetAsync(command.AncillaryProductId, cancellationToken)
                          ?? throw ExceptionFactory.AncillaryProductNotFound();

            var now = _clock.GetDateTime();
            var wasDraft = product.Status == AncillaryProductStatus.Draft;
            var subCode = product.Document.Rfisc is null
                ? null
                : await _subCodes.FindActiveAsync(product.OwnerAirlineId, product.Document.Rfisc, cancellationToken);

            product.Activate(subCode, now);

            var previous = wasDraft
                ? await _products.FindOfferedVersionAsync(product.OwnerAirlineId, product.ProductRef, cancellationToken)
                : null;

            if (previous is not null)
            {
                previous.Retire(now);
                await _synchronizer.ProjectAsync(previous.ToReadModelSnapshot(), cancellationToken);
            }

            await _synchronizer.ProjectAsync(product.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ActivateAncillaryProductResult(
                product.Id,
                product.OwnerAirlineId,
                product.ProductRef,
                product.Version,
                product.Status,
                previous?.Id);
        }
    }
}
