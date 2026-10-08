using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionCustomer : Entity<long>
    {
        private ProvisionCustomer()
        {
        }

        internal ProvisionCustomer(long id, long ancillaryProvisionId, long customerId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(customerId);
        }

        public long AncillaryProvisionId { get; private set; }

        public long CustomerId { get; private set; }

        internal void Change(long customerId)
        {
            if (customerId <= 0)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionCustomer)}.{nameof(CustomerId)}");

            CustomerId = customerId;
        }

        internal bool SameAs(ProvisionCustomer other) => CustomerId == other.CustomerId;
    }
}
