using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeRestriction.Backoffice
{
    public sealed class BackofficeAddProvisionDayTimeRestrictionCommandHandler
        : IRequestHandler<BackofficeAddProvisionDayTimeRestrictionCommand, ProvisionConditionRowResult>
    {
        private readonly IAddProvisionDayTimeRestrictionService _service;

        public BackofficeAddProvisionDayTimeRestrictionCommandHandler(IAddProvisionDayTimeRestrictionService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeAddProvisionDayTimeRestrictionCommand command, CancellationToken cancellationToken)
            => _service.AddAsync(command, cancellationToken);
    }
}
