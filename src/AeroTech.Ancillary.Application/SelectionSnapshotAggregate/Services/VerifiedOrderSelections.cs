namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public sealed record VerifiedOrderSelections(int OrderCommercialVersion, IReadOnlyList<VerifiedSelection> Selections);
}
