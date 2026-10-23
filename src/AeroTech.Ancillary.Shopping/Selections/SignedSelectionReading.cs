using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public sealed record SignedSelectionReading(SignedSelectionStatus Status, SignedSelection? Selection)
    {
        public static readonly SignedSelectionReading Rejected = new(SignedSelectionStatus.Rejected, null);
    }
}
