using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public abstract record SubmittedSelection(string Id)
    {
        public abstract SelectionIdKind Kind { get; }
    }
}
