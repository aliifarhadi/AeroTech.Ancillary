using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSalesRestrictions.Backoffice
{
    public sealed class BackofficeChangeProvisionSalesRestrictionsCommandHandler
        : IRequestHandler<BackofficeChangeProvisionSalesRestrictionsCommand, ProvisionResult>
    {
        private readonly IChangeProvisionSalesRestrictionsService _service;

        public BackofficeChangeProvisionSalesRestrictionsCommandHandler(IChangeProvisionSalesRestrictionsService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionSalesRestrictionsCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
