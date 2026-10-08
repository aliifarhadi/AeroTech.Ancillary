using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionEligibleAgeBand : Entity<long>
    {
        private ProvisionEligibleAgeBand()
        {
        }

        internal ProvisionEligibleAgeBand(long id, long ancillaryProvisionId, long provisionPassengerEligibilityRuleId, ProvisionAgeBandArgs args)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionPassengerEligibilityRuleId = provisionPassengerEligibilityRuleId;
            Change(args);
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionPassengerEligibilityRuleId { get; private set; }

        public int AgeFromInclusive { get; private set; }

        public int? AgeToExclusive { get; private set; }

        internal void Change(ProvisionAgeBandArgs args)
        {
            Require(args.AgeFromInclusive >= 0, nameof(AgeFromInclusive));
            Require(args.AgeToExclusive is null || args.AgeToExclusive > args.AgeFromInclusive, nameof(AgeToExclusive));

            AgeFromInclusive = args.AgeFromInclusive;
            AgeToExclusive = args.AgeToExclusive;
        }

        internal bool SameAs(ProvisionEligibleAgeBand other)
            => AgeFromInclusive == other.AgeFromInclusive && AgeToExclusive == other.AgeToExclusive;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionEligibleAgeBand)}.{field}");
        }
    }
}
