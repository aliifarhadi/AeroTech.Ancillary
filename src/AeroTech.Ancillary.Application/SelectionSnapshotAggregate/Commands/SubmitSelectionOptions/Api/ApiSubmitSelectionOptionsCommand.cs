using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions.Api
{
    public sealed record ApiSubmitSelectionOptionsCommand(
        string Reference,
        string? OfferId,
        AncillaryProfile Family,
        ISelectionOptions Options) : IRequest<SelectionSnapshotResult>, ISubmitSelectionOptionsCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.Api;
    }
}
