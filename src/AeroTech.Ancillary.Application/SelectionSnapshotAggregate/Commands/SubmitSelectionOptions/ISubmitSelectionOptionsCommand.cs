using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions
{
    public interface ISubmitSelectionOptionsCommand
    {
        AuthorizationSurface Surface { get; }

        string Reference { get; }

        string? OfferId { get; }

        AncillaryProfile Family { get; }

        ISelectionOptions Options { get; }
    }
}
