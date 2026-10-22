namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public sealed class AncillaryHoldOptions
    {
        public const string SectionName = "Holds";

        public int LockExpirySeconds { get; set; }

        public int ExpiryPollIntervalSeconds { get; set; }

        public int ExpiryBatchSize { get; set; }
    }
}
