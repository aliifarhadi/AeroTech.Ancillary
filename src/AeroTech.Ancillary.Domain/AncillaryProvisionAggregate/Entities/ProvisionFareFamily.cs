using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionFareFamily : Entity<long>
    {
        private ProvisionFareFamily()
        {
        }

        internal ProvisionFareFamily(long id, long ancillaryProvisionId, long provisionFareApplicationRuleId, long fareFamilyId)
        {
            if (!(fareFamilyId > 0))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionFareFamily)}.{nameof(FareFamilyId)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionFareApplicationRuleId = provisionFareApplicationRuleId;
            FareFamilyId = fareFamilyId;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionFareApplicationRuleId { get; private set; }

        public long FareFamilyId { get; private set; }

        internal bool SameAs(ProvisionFareFamily other) => FareFamilyId == other.FareFamilyId;
    }
}
