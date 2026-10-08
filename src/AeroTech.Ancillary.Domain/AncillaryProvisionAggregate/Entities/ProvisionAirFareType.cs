using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionAirFareType : Entity<long>
    {
        private ProvisionAirFareType()
        {
        }

        internal ProvisionAirFareType(long id, long ancillaryProvisionId, long provisionFareApplicationRuleId, AirFareType airFareType)
        {
            if (!(Enum.IsDefined(airFareType)))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionAirFareType)}.{nameof(AirFareType)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionFareApplicationRuleId = provisionFareApplicationRuleId;
            AirFareType = airFareType;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionFareApplicationRuleId { get; private set; }

        public AirFareType AirFareType { get; private set; }

        internal bool SameAs(ProvisionAirFareType other) => AirFareType == other.AirFareType;
    }
}
