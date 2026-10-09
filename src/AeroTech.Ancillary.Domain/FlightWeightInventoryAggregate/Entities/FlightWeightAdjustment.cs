using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Entities
{
    public sealed class FlightWeightAdjustment : Entity<long>
    {
        private FlightWeightAdjustment()
        {
        }

        internal FlightWeightAdjustment(
            long id,
            long flightWeightInventoryId,
            decimal previousKg,
            decimal newKg,
            string reasonCode,
            long actorId,
            string correlationId,
            DateTimeOffset occurredAt,
            long expectedVersion,
            long resultingVersion)
        {
            Id = id;
            FlightWeightInventoryId = flightWeightInventoryId;
            PreviousKg = previousKg;
            NewKg = newKg;
            ReasonCode = reasonCode;
            ActorId = actorId;
            CorrelationId = correlationId;
            OccurredAt = occurredAt;
            ExpectedVersion = expectedVersion;
            ResultingVersion = resultingVersion;
        }

        public long FlightWeightInventoryId { get; private set; }

        public decimal PreviousKg { get; private set; }

        public decimal NewKg { get; private set; }

        public string ReasonCode { get; private set; } = default!;

        public long ActorId { get; private set; }

        public string CorrelationId { get; private set; } = default!;

        public DateTimeOffset OccurredAt { get; private set; }

        public long ExpectedVersion { get; private set; }

        public long ResultingVersion { get; private set; }

        internal bool IsRequest(decimal newKg, string reasonCode, long actorId, long expectedVersion)
            => NewKg == newKg && ReasonCode == reasonCode && ActorId == actorId && ExpectedVersion == expectedVersion;
    }
}
