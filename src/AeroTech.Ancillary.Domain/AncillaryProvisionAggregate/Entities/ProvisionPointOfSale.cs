using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionPointOfSale : Entity<long>
    {
        private ProvisionPointOfSale()
        {
        }

        internal ProvisionPointOfSale(long id, long ancillaryProvisionId, long provisionSalesRestrictionsRuleId, long pointOfSaleId)
        {
            if (!(pointOfSaleId > 0))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionPointOfSale)}.{nameof(PointOfSaleId)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionSalesRestrictionsRuleId = provisionSalesRestrictionsRuleId;
            PointOfSaleId = pointOfSaleId;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionSalesRestrictionsRuleId { get; private set; }

        public long PointOfSaleId { get; private set; }

        internal bool SameAs(ProvisionPointOfSale other) => PointOfSaleId == other.PointOfSaleId;
    }
}
