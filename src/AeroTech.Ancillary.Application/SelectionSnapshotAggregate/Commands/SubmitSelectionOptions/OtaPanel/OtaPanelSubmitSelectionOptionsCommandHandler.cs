using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions.OtaPanel
{
    public sealed class OtaPanelSubmitSelectionOptionsCommandHandler : IRequestHandler<OtaPanelSubmitSelectionOptionsCommand, SelectionSnapshotResult>
    {
        private readonly ISubmitSelectionOptionsService _service;

        public OtaPanelSubmitSelectionOptionsCommandHandler(ISubmitSelectionOptionsService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(OtaPanelSubmitSelectionOptionsCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
