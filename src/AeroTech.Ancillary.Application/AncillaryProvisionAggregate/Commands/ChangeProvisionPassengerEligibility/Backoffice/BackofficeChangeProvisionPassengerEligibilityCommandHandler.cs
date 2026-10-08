using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPassengerEligibility.Backoffice
{
    public sealed class BackofficeChangeProvisionPassengerEligibilityCommandHandler
        : IRequestHandler<BackofficeChangeProvisionPassengerEligibilityCommand, ProvisionResult>
    {
        private readonly IChangeProvisionPassengerEligibilityService _service;

        public BackofficeChangeProvisionPassengerEligibilityCommandHandler(IChangeProvisionPassengerEligibilityService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionPassengerEligibilityCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
