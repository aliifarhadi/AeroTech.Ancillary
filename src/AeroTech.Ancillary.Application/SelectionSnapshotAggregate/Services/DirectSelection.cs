using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public sealed record DirectSelection(string Id, int? Quantity) : SubmittedSelection(Id)
    {
        public override SelectionIdKind Kind => SelectionIdKind.Direct;
    }
}
