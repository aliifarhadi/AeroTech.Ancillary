using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionFareFamily : Entity<long>
    {
        private ProvisionFareFamily()
        {
        }

        internal ProvisionFareFamily(long id, long ancillaryProvisionId, long fareFamilyId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(fareFamilyId);
        }

        public long AncillaryProvisionId { get; private set; }

        public long FareFamilyId { get; private set; }

        internal void Change(long fareFamilyId)
        {
            if (fareFamilyId <= 0)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionFareFamily)}.{nameof(FareFamilyId)}");

            FareFamilyId = fareFamilyId;
        }

        internal bool SameAs(ProvisionFareFamily other) => FareFamilyId == other.FareFamilyId;
    }
}
