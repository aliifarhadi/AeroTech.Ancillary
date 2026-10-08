using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionFlight : Entity<long>
    {
        private ProvisionFlight()
        {
        }

        internal ProvisionFlight(long id, long ancillaryProvisionId, long flightId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(flightId);
        }

        public long AncillaryProvisionId { get; private set; }

        public long FlightId { get; private set; }

        internal void Change(long flightId)
        {
            if (flightId <= 0)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionFlight)}.{nameof(FlightId)}");

            FlightId = flightId;
        }

        internal bool SameAs(ProvisionFlight other) => FlightId == other.FlightId;
    }
}
