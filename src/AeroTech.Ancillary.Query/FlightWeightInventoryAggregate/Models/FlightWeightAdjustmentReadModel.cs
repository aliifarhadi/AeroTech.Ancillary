namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Models
{
    public sealed class FlightWeightAdjustmentReadModel
    {
        public long Id { get; set; }

        public long FlightWeightInventoryId { get; set; }

        public decimal PreviousKg { get; set; }

        public decimal NewKg { get; set; }

        public string ReasonCode { get; set; } = default!;

        public long ActorId { get; set; }

        public string CorrelationId { get; set; } = default!;

        public DateTimeOffset OccurredAt { get; set; }

        public long ExpectedVersion { get; set; }

        public long ResultingVersion { get; set; }
    }
}
