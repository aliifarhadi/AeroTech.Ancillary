using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeRestriction.Backoffice
{
    public sealed class BackofficeRemoveProvisionDayTimeRestrictionCommandHandler
        : IRequestHandler<BackofficeRemoveProvisionDayTimeRestrictionCommand, ProvisionConditionRowResult>
    {
        private readonly IRemoveProvisionDayTimeRestrictionService _service;

        public BackofficeRemoveProvisionDayTimeRestrictionCommandHandler(IRemoveProvisionDayTimeRestrictionService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeRemoveProvisionDayTimeRestrictionCommand command, CancellationToken cancellationToken)
            => _service.RemoveAsync(command, cancellationToken);
    }
}
