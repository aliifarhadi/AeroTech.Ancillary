using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionOriginAirport : Entity<long>
    {
        private ProvisionOriginAirport()
        {
        }

        internal ProvisionOriginAirport(long id, long ancillaryProvisionId, long provisionGeographyRuleId, int airportId)
        {
            if (!(airportId > 0))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionOriginAirport)}.{nameof(AirportId)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionGeographyRuleId = provisionGeographyRuleId;
            AirportId = airportId;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionGeographyRuleId { get; private set; }

        public int AirportId { get; private set; }

        internal bool SameAs(ProvisionOriginAirport other) => AirportId == other.AirportId;
    }
}
