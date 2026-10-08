using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionCabinClass : Entity<long>
    {
        private ProvisionCabinClass()
        {
        }

        internal ProvisionCabinClass(long id, long ancillaryProvisionId, int cabinClassId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(cabinClassId);
        }

        public long AncillaryProvisionId { get; private set; }

        public int CabinClassId { get; private set; }

        internal void Change(int cabinClassId)
        {
            if (cabinClassId <= 0)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionCabinClass)}.{nameof(CabinClassId)}");

            CabinClassId = cabinClassId;
        }

        internal bool SameAs(ProvisionCabinClass other) => CabinClassId == other.CabinClassId;
    }
}
