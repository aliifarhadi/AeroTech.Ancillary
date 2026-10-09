using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionAdvancePurchaseRule : Entity<long>
    {
        private ProvisionAdvancePurchaseRule()
        {
        }

        private ProvisionAdvancePurchaseRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public int MinimumPeriod { get; private set; }

        public int? MaximumPeriod { get; private set; }

        public TimeUnit Unit { get; private set; }

        public bool SameTimeAsTicketed { get; private set; }

        internal static Func<ProvisionAdvancePurchaseRule?> Plan(
            ProvisionAdvancePurchaseRule? stored,
            long ancillaryProvisionId,
            ProvisionAdvancePurchaseArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null)
                return () => null;

            Require(args.MinimumPeriod >= 0, nameof(MinimumPeriod));
            Require(args.MaximumPeriod is null || args.MaximumPeriod >= args.MinimumPeriod, nameof(MaximumPeriod));
            Require(Enum.IsDefined(args.Unit), nameof(Unit));

            var rule = stored ?? new ProvisionAdvancePurchaseRule(idGenerator.NewId(), ancillaryProvisionId);

            return () =>
            {
                rule.MinimumPeriod = args.MinimumPeriod;
                rule.MaximumPeriod = args.MaximumPeriod;
                rule.Unit = args.Unit;
                rule.SameTimeAsTicketed = args.SameTimeAsTicketed;

                return rule;
            };
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionAdvancePurchaseRule)}.{field}");
        }
    }
}
