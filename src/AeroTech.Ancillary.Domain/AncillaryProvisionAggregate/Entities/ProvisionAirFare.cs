using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionAirFare : Entity<long>
    {
        private ProvisionAirFare()
        {
        }

        internal ProvisionAirFare(long id, long ancillaryProvisionId, long airFareId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(airFareId);
        }

        public long AncillaryProvisionId { get; private set; }

        public long AirFareId { get; private set; }

        internal void Change(long airFareId)
        {
            if (airFareId <= 0)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionAirFare)}.{nameof(AirFareId)}");

            AirFareId = airFareId;
        }

        internal bool SameAs(ProvisionAirFare other) => AirFareId == other.AirFareId;
    }
}
