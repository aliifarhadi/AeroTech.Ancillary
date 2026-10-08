using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionMarketingAirline : Entity<long>
    {
        private ProvisionMarketingAirline()
        {
        }

        internal ProvisionMarketingAirline(long id, long ancillaryProvisionId, int airlineId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(airlineId);
        }

        public long AncillaryProvisionId { get; private set; }

        public int AirlineId { get; private set; }

        internal void Change(int airlineId)
        {
            if (airlineId <= 0)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionMarketingAirline)}.{nameof(AirlineId)}");

            AirlineId = airlineId;
        }

        internal bool SameAs(ProvisionMarketingAirline other) => AirlineId == other.AirlineId;
    }
}
