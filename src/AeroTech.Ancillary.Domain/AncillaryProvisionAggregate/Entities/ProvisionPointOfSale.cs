using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionPointOfSale : Entity<long>
    {
        private ProvisionPointOfSale()
        {
        }

        internal ProvisionPointOfSale(long id, long ancillaryProvisionId, long pointOfSaleId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(pointOfSaleId);
        }

        public long AncillaryProvisionId { get; private set; }

        public long PointOfSaleId { get; private set; }

        internal void Change(long pointOfSaleId)
        {
            if (pointOfSaleId <= 0)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionPointOfSale)}.{nameof(PointOfSaleId)}");

            PointOfSaleId = pointOfSaleId;
        }

        internal bool SameAs(ProvisionPointOfSale other) => PointOfSaleId == other.PointOfSaleId;
    }
}
