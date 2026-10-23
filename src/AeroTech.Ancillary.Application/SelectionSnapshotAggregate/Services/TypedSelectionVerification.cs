using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public sealed record TypedSelectionVerification(
        AuthorizationSurface Surface,
        string Reference,
        AncillaryProfile Family,
        ISelectionOptions Options);
}
