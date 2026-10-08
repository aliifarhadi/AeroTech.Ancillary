using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionCoverageCountry : Entity<long>
    {
        private ProvisionCoverageCountry()
        {
        }

        internal ProvisionCoverageCountry(long id, long ancillaryProvisionId, long provisionGeographyRuleId, int countryId)
        {
            if (!(countryId > 0))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionCoverageCountry)}.{nameof(CountryId)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionGeographyRuleId = provisionGeographyRuleId;
            CountryId = countryId;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionGeographyRuleId { get; private set; }

        public int CountryId { get; private set; }

        internal bool SameAs(ProvisionCoverageCountry other) => CountryId == other.CountryId;
    }
}
