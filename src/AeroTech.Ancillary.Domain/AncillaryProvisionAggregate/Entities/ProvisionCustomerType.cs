using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionCustomerType : Entity<long>
    {
        private ProvisionCustomerType()
        {
        }

        internal ProvisionCustomerType(long id, long ancillaryProvisionId, CustomerType customerType)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(customerType);
        }

        public long AncillaryProvisionId { get; private set; }

        public CustomerType CustomerType { get; private set; }

        internal void Change(CustomerType customerType)
        {
            if (!Enum.IsDefined(customerType))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionCustomerType)}.{nameof(CustomerType)}");

            CustomerType = customerType;
        }

        internal bool SameAs(ProvisionCustomerType other) => CustomerType == other.CustomerType;
    }
}
