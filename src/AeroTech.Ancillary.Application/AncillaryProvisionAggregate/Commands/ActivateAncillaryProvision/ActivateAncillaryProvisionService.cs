using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision
{
    public sealed class ActivateAncillaryProvisionService : IActivateAncillaryProvisionService
    {
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryProvisionQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public ActivateAncillaryProvisionService(
            IAncillaryProvisionRepository provisions,
            IAncillaryProvisionQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _provisions = provisions;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<ProvisionResult> ActivateAsync(IActivateAncillaryProvisionCommand command, CancellationToken cancellationToken = default)
        {
            var provision = await _provisions.GetAsync(command.ProvisionId, cancellationToken)
                            ?? throw ExceptionFactory.ProvisionNotFound();

            var now = _clock.GetDateTime();
            var superseded = await _provisions.FindActiveAtSequenceAsync(provision.ServiceDefinitionId, provision.Sequence, cancellationToken);

            if (superseded is not null && superseded.Id != provision.Id)
            {
                superseded.Supersede(now);
                await _synchronizer.ProjectAsync(superseded.ToReadModelSnapshot(), cancellationToken);
            }

            provision.Activate(now);

            await _synchronizer.ProjectAsync(provision.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ProvisionResult(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Sequence,
                provision.CoverageScope,
                provision.Outcome.Disposition,
                provision.Status);
        }
    }
}
