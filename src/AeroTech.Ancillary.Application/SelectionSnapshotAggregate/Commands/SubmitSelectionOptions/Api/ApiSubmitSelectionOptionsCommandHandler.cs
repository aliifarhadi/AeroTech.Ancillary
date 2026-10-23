using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions.Api
{
    public sealed class ApiSubmitSelectionOptionsCommandHandler : IRequestHandler<ApiSubmitSelectionOptionsCommand, SelectionSnapshotResult>
    {
        private readonly ISubmitSelectionOptionsService _service;

        public ApiSubmitSelectionOptionsCommandHandler(ISubmitSelectionOptionsService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(ApiSubmitSelectionOptionsCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
