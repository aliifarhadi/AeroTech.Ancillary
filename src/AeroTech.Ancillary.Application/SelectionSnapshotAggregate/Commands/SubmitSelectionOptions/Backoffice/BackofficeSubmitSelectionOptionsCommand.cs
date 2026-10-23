using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions.Backoffice
{
    public sealed record BackofficeSubmitSelectionOptionsCommand(
        string Reference,
        string? OfferId,
        AncillaryProfile Family,
        ISelectionOptions Options) : IRequest<SelectionSnapshotResult>, ISubmitSelectionOptionsCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.Backoffice;
    }
}
