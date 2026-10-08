using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionAirFare : Entity<long>
    {
        private ProvisionAirFare()
        {
        }

        internal ProvisionAirFare(long id, long ancillaryProvisionId, long provisionFareApplicationRuleId, long airFareId)
        {
            if (!(airFareId > 0))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionAirFare)}.{nameof(AirFareId)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionFareApplicationRuleId = provisionFareApplicationRuleId;
            AirFareId = airFareId;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionFareApplicationRuleId { get; private set; }

        public long AirFareId { get; private set; }

        internal bool SameAs(ProvisionAirFare other) => AirFareId == other.AirFareId;
    }
}
