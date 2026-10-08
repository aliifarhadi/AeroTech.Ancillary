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

        internal ProvisionPassengerType(long id, long ancillaryProvisionId, PassengerTypeCode passengerTypeCode)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(passengerTypeCode);
        }

        public long AncillaryProvisionId { get; private set; }

        public PassengerTypeCode PassengerTypeCode { get; private set; }

        internal void Change(PassengerTypeCode passengerTypeCode)
        {
            if (!Enum.IsDefined(passengerTypeCode))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionPassengerType)}.{nameof(PassengerTypeCode)}");

            PassengerTypeCode = passengerTypeCode;
        }

        internal bool SameAs(ProvisionPassengerType other) => PassengerTypeCode == other.PassengerTypeCode;
    }
}
