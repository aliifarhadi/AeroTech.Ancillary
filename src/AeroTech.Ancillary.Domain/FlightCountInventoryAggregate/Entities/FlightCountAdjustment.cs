using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Entities
{
    public sealed class FlightCountAdjustment : Entity<long>
    {
        private FlightCountAdjustment()
        {
        }

        internal FlightCountAdjustment(
            long id,
            long flightCountInventoryId,
            int previousTotal,
            int newTotal,
            string reasonCode,
            long actorId,
            string correlationId,
            DateTimeOffset occurredAt,
            long expectedVersion,
            long resultingVersion)
        {
            Id = id;
            FlightCountInventoryId = flightCountInventoryId;
            PreviousTotal = previousTotal;
            NewTotal = newTotal;
            ReasonCode = reasonCode;
            ActorId = actorId;
            CorrelationId = correlationId;
            OccurredAt = occurredAt;
            ExpectedVersion = expectedVersion;
            ResultingVersion = resultingVersion;
        }

        public long FlightCountInventoryId { get; private set; }

        public int PreviousTotal { get; private set; }

        public int NewTotal { get; private set; }

        public string ReasonCode { get; private set; } = default!;

        public long ActorId { get; private set; }

        public string CorrelationId { get; private set; } = default!;

        public DateTimeOffset OccurredAt { get; private set; }

        public long ExpectedVersion { get; private set; }

        public long ResultingVersion { get; private set; }

        internal bool IsRequest(int newTotal, string reasonCode, long actorId, long expectedVersion)
            => NewTotal == newTotal && ReasonCode == reasonCode && ActorId == actorId && ExpectedVersion == expectedVersion;
    }
}
