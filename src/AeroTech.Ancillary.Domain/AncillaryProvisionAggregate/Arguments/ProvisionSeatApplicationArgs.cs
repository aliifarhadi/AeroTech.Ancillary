namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionSeatApplicationArgs(
        IReadOnlyList<string> SeatNumbers,
        IReadOnlyList<string> SeatCharacteristicCodes)
    {
        public bool IsEmpty
            => SeatNumbers.Count == 0 && SeatCharacteristicCodes.Count == 0;
    }
}
