using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ReviseAncillaryProduct
{
    public sealed class ReviseAncillaryProductService : IReviseAncillaryProductService
    {
        private readonly IAncillaryProductRepository _products;
        private readonly IAncillaryProductQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public ReviseAncillaryProductService(
            IAncillaryProductRepository products,
            IAncillaryProductQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _products = products;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<AncillaryProductResult> ReviseAsync(IReviseAncillaryProductCommand command, CancellationToken cancellationToken = default)
        {
            var source = await _products.GetAsync(command.AncillaryProductId, cancellationToken)
                         ?? throw ExceptionFactory.AncillaryProductNotFound();

            var highestVersion = await _products.HighestVersionAsync(source.OwnerAirlineId, source.ProductRef, cancellationToken);
            var draft = source.Revise(_idGenerator.NewId(), highestVersion + 1, _clock.GetDateTime());

            if (await _products.DraftExistsAsync(source.OwnerAirlineId, source.ProductRef, cancellationToken))
                throw ExceptionFactory.AncillaryProductDraftAlreadyExists();

            await _products.AddAsync(draft, cancellationToken);
            await _synchronizer.ProjectAsync(draft.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AncillaryProductResult(draft.Id, draft.OwnerAirlineId, draft.ProductRef, draft.Version, draft.Status);
        }
    }
}
