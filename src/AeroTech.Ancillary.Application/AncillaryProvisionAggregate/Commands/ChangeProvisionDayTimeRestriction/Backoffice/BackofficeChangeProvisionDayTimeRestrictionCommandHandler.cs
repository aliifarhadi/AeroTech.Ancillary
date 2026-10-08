using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeRestriction.Backoffice
{
    public sealed class BackofficeChangeProvisionDayTimeRestrictionCommandHandler
        : IRequestHandler<BackofficeChangeProvisionDayTimeRestrictionCommand, ProvisionConditionRowResult>
    {
        private readonly IChangeProvisionDayTimeRestrictionService _service;

        public BackofficeChangeProvisionDayTimeRestrictionCommandHandler(IChangeProvisionDayTimeRestrictionService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeChangeProvisionDayTimeRestrictionCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
