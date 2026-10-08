using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionCustomer : Entity<long>
    {
        private ProvisionCustomer()
        {
        }

        internal ProvisionCustomer(long id, long ancillaryProvisionId, long provisionSalesRestrictionsRuleId, long customerId)
        {
            if (!(customerId > 0))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionCustomer)}.{nameof(CustomerId)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionSalesRestrictionsRuleId = provisionSalesRestrictionsRuleId;
            CustomerId = customerId;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionSalesRestrictionsRuleId { get; private set; }

        public long CustomerId { get; private set; }

        internal bool SameAs(ProvisionCustomer other) => CustomerId == other.CustomerId;
    }
}
