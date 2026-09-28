namespace AeroTech.Ancillary.ReferenceData.ReadModels
{
    public sealed class ReferenceSyncState
    {
        public string Id { get; set; } = default!;
        public DateTimeOffset? LastSync { get; set; }
    }
}
