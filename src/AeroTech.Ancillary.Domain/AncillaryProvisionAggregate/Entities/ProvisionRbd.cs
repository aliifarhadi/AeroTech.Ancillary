using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionRbd : Entity<long>
    {
        private ProvisionRbd()
        {
        }

        internal ProvisionRbd(long id, long ancillaryProvisionId, long provisionFareApplicationRuleId, long rbdId)
        {
            if (!(rbdId > 0))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionRbd)}.{nameof(RbdId)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionFareApplicationRuleId = provisionFareApplicationRuleId;
            RbdId = rbdId;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionFareApplicationRuleId { get; private set; }

        public long RbdId { get; private set; }

        internal bool SameAs(ProvisionRbd other) => RbdId == other.RbdId;
    }
}
