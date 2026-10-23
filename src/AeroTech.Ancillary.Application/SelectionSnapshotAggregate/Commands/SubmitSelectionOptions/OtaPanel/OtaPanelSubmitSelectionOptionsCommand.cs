using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions.OtaPanel
{
    public sealed record OtaPanelSubmitSelectionOptionsCommand(
        string Reference,
        string? OfferId,
        AncillaryProfile Family,
        ISelectionOptions Options) : IRequest<SelectionSnapshotResult>, ISubmitSelectionOptionsCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.OtaPanel;
    }
}
