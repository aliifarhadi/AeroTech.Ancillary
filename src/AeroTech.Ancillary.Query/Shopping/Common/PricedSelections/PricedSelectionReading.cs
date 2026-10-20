using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common.PricedSelections
{
    public sealed record PricedSelectionReading(SelectionResolutionStatus Status, PricedSelection? Selection, DateTimeOffset? ExpiresAtUtc)
    {
        public static readonly PricedSelectionReading Rejected = new(SelectionResolutionStatus.Rejected, null, null);
    }
}
