using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions.Backoffice
{
    public sealed class BackofficeSubmitSelectionOptionsCommandHandler : IRequestHandler<BackofficeSubmitSelectionOptionsCommand, SelectionSnapshotResult>
    {
        private readonly ISubmitSelectionOptionsService _service;

        public BackofficeSubmitSelectionOptionsCommandHandler(ISubmitSelectionOptionsService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(BackofficeSubmitSelectionOptionsCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
