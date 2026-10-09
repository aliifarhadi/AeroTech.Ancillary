using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionAssistedTravelRule : Entity<long>
    {
        private ProvisionAssistedTravelRule()
        {
        }

        private ProvisionAssistedTravelRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public int? MinimumLeadTimeMinutes { get; private set; }

        public MinorConnectionPolicy? ConnectionPolicy { get; private set; }

        public bool? MedicalApprovalRequired { get; private set; }

        internal static Func<ProvisionAssistedTravelRule?> Plan(
            ProvisionAssistedTravelRule? stored,
            long ancillaryProvisionId,
            ProvisionAssistedTravelRuleArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null)
                return () => null;

            Require(args.MinimumLeadTimeMinutes is null or >= 0, nameof(MinimumLeadTimeMinutes));
            Require(args.ConnectionPolicy is null || Enum.IsDefined(args.ConnectionPolicy.Value), nameof(ConnectionPolicy));
            Require(args.MinimumLeadTimeMinutes is not null || args.ConnectionPolicy is not null || args.MedicalApprovalRequired is not null, nameof(MinimumLeadTimeMinutes));

            var rule = stored ?? new ProvisionAssistedTravelRule(idGenerator.NewId(), ancillaryProvisionId);

            return () =>
            {
                rule.MinimumLeadTimeMinutes = args.MinimumLeadTimeMinutes;
                rule.ConnectionPolicy = args.ConnectionPolicy;
                rule.MedicalApprovalRequired = args.MedicalApprovalRequired;

                return rule;
            };
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionAssistedTravelRule)}.{field}");
        }
    }
}
