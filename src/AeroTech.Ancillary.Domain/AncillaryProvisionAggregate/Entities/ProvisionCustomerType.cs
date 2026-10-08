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

        internal ProvisionCustomerType(long id, long ancillaryProvisionId, long provisionSalesRestrictionsRuleId, CustomerType customerType)
        {
            if (!(Enum.IsDefined(customerType)))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionCustomerType)}.{nameof(CustomerType)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionSalesRestrictionsRuleId = provisionSalesRestrictionsRuleId;
            CustomerType = customerType;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionSalesRestrictionsRuleId { get; private set; }

        public CustomerType CustomerType { get; private set; }

        internal bool SameAs(ProvisionCustomerType other) => CustomerType == other.CustomerType;
    }
}
