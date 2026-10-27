using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;

namespace AeroTech.Ancillary.Shopping.Resolution
{
    public sealed record ResolvedSelections(SelectionSnapshot Selection, IReadOnlyList<ResolvedSelection> Lines);
}
