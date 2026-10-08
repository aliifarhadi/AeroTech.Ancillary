using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionPassengerType : Entity<long>
    {
        private ProvisionPassengerType()
        {
        }

        internal ProvisionPassengerType(long id, long ancillaryProvisionId, long provisionPassengerEligibilityRuleId, PassengerTypeCode passengerTypeCode)
        {
            if (!(Enum.IsDefined(passengerTypeCode)))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionPassengerType)}.{nameof(PassengerTypeCode)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionPassengerEligibilityRuleId = provisionPassengerEligibilityRuleId;
            PassengerTypeCode = passengerTypeCode;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionPassengerEligibilityRuleId { get; private set; }

        public PassengerTypeCode PassengerTypeCode { get; private set; }

        internal bool SameAs(ProvisionPassengerType other) => PassengerTypeCode == other.PassengerTypeCode;
    }
}
