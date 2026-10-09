namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Models
{
    public sealed class AirportSlotAdjustmentReadModel
    {
        public long Id { get; set; }

        public long AirportSlotInventoryId { get; set; }

        public int PreviousTotal { get; set; }

        public int NewTotal { get; set; }

        public string ReasonCode { get; set; } = default!;

        public long ActorId { get; set; }

        public string CorrelationId { get; set; } = default!;

        public DateTimeOffset OccurredAt { get; set; }

        public long ExpectedVersion { get; set; }

        public long ResultingVersion { get; set; }
    }
}
