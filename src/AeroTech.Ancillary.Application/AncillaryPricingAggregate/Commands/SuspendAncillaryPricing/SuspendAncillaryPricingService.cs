using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SuspendAncillaryPricing
{
    public sealed class SuspendAncillaryPricingService : ISuspendAncillaryPricingService
    {
        private readonly IAncillaryPricingRepository _pricings;
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryPricingQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public SuspendAncillaryPricingService(
            IAncillaryPricingRepository pricings,
            IAncillaryProvisionRepository provisions,
            IAncillaryPricingQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _pricings = pricings;
            _provisions = provisions;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<PricingResult> SuspendAsync(ISuspendAncillaryPricingCommand command, CancellationToken cancellationToken = default)
        {
            var pricing = await _pricings.GetAsync(command.PricingId, cancellationToken)
                          ?? throw ExceptionFactory.PricingNotFound();
            var provision = await _provisions.GetAsync(pricing.AncillaryProvisionId, cancellationToken)
                            ?? throw ExceptionFactory.PricingProvisionNotFound();

            if (pricing.Status == PricingStatus.Active && provision.Status == ProvisionStatus.Active)
                throw ExceptionFactory.PricingRequiredByActiveProvision();

            pricing.Suspend(_clock.GetDateTime());

            await _synchronizer.ProjectAsync(pricing.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return pricing.ToResult();
        }
    }
}
