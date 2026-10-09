namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Models
{
    public sealed class FlightCountAdjustmentReadModel
    {
        public long Id { get; set; }

        public long FlightCountInventoryId { get; set; }

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
