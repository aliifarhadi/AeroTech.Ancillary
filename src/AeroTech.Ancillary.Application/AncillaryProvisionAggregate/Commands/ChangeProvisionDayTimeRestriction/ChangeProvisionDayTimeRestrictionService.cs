using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Projection;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeRestriction
{
    public sealed class ChangeProvisionDayTimeRestrictionService : IChangeProvisionDayTimeRestrictionService
    {
        private readonly IAncillaryProvisionRepository _provisions;
        private readonly IAncillaryProvisionQueryDbSynchronizer _synchronizer;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeProvisionDayTimeRestrictionService(
            IAncillaryProvisionRepository provisions,
            IAncillaryProvisionQueryDbSynchronizer synchronizer,
            IUnitOfWork unitOfWork)
        {
            _provisions = provisions;
            _synchronizer = synchronizer;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProvisionConditionRowResult> ChangeAsync(IChangeProvisionDayTimeRestrictionCommand command, CancellationToken cancellationToken = default)
        {
            var provision = await _provisions.GetAsync(command.ProvisionId, cancellationToken)
                            ?? throw ExceptionFactory.ProvisionNotFound();

            provision.ChangeDayTimeRestriction(command.RowId, new ProvisionDayTimeRestrictionArgs(command.DayOfWeek, command.StartTime, command.EndTime, command.Effect));

            await _synchronizer.ProjectAsync(provision.ToReadModelSnapshot(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ProvisionConditionRowResult(provision.Id, command.RowId);
        }
    }
}
