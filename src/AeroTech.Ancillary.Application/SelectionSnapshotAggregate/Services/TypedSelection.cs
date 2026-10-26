using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public sealed record TypedSelection(string Id, AncillaryProfile Family, ISelectionOptions Options) : SubmittedSelection(Id)
    {
        public override SelectionIdKind Kind => SelectionIdKind.Reference;
    }
}
