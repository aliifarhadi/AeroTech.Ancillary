using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionAircraft : Entity<long>
    {
        private ProvisionAircraft()
        {
        }

        internal ProvisionAircraft(long id, long ancillaryProvisionId, int aircraftId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(aircraftId);
        }

        public long AncillaryProvisionId { get; private set; }

        public int AircraftId { get; private set; }

        internal void Change(int aircraftId)
        {
            if (aircraftId <= 0)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionAircraft)}.{nameof(AircraftId)}");

            AircraftId = aircraftId;
        }

        internal bool SameAs(ProvisionAircraft other) => AircraftId == other.AircraftId;
    }
}
