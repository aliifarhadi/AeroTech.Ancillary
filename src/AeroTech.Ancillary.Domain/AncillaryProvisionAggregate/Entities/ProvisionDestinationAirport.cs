using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionDestinationAirport : Entity<long>
    {
        private ProvisionDestinationAirport()
        {
        }

        internal ProvisionDestinationAirport(long id, long ancillaryProvisionId, int airportId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(airportId);
        }

        public long AncillaryProvisionId { get; private set; }

        public int AirportId { get; private set; }

        internal void Change(int airportId)
        {
            if (airportId <= 0)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionDestinationAirport)}.{nameof(AirportId)}");

            AirportId = airportId;
        }

        internal bool SameAs(ProvisionDestinationAirport other) => AirportId == other.AirportId;
    }
}
