using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionAircraft : Entity<long>
    {
        private ProvisionAircraft()
        {
        }

        internal ProvisionAircraft(long id, long ancillaryProvisionId, long provisionFlightApplicationRuleId, int aircraftId)
        {
            if (!(aircraftId > 0))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionAircraft)}.{nameof(AircraftId)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionFlightApplicationRuleId = provisionFlightApplicationRuleId;
            AircraftId = aircraftId;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionFlightApplicationRuleId { get; private set; }

        public int AircraftId { get; private set; }

        internal bool SameAs(ProvisionAircraft other) => AircraftId == other.AircraftId;
    }
}
