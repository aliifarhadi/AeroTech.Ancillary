using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReviseAncillaryPricing
{
    public sealed class ReviseAncillaryPricingService : IReviseAncillaryPricingService
    {
        private readonly IAncillaryPricingRepository _pricings;
        private readonly IAncillaryPricingQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public ReviseAncillaryPricingService(
            IAncillaryPricingRepository pricings,
            IAncillaryPricingQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _pricings = pricings;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<PricingResult> ReviseAsync(IReviseAncillaryPricingCommand command, CancellationToken cancellationToken = default)
        {
            var pricing = await _pricings.GetAsync(command.PricingId, cancellationToken)
                          ?? throw ExceptionFactory.PricingNotFound();
            var version = await _pricings.MaxVersionAsync(pricing.AncillaryProvisionId, cancellationToken) + 1;
            var revision = pricing.Revise(_idGenerator.NewId(), version, _idGenerator, _clock.GetDateTime());

            await _pricings.AddAsync(revision, cancellationToken);
            await _synchronizer.ProjectAsync(revision.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return revision.ToResult();
        }
    }
}
