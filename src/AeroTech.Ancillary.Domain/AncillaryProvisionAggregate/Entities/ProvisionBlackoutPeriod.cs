using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionBlackoutPeriod : Entity<long>
    {
        private ProvisionBlackoutPeriod()
        {
        }

        internal ProvisionBlackoutPeriod(long id, long ancillaryProvisionId, long provisionTravelDateRuleId, ProvisionDatePeriodArgs args)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionTravelDateRuleId = provisionTravelDateRuleId;
            Change(args);
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionTravelDateRuleId { get; private set; }

        public DateOnly StartDate { get; private set; }

        public DateOnly EndDate { get; private set; }

        internal void Change(ProvisionDatePeriodArgs args)
        {
            Require(args.StartDate != DateOnly.MinValue, nameof(StartDate));
            Require(args.StartDate <= args.EndDate, nameof(EndDate));

            StartDate = args.StartDate;
            EndDate = args.EndDate;
        }

        internal bool SameAs(ProvisionDatePeriodArgs args) => StartDate == args.StartDate && EndDate == args.EndDate;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionBlackoutPeriod)}.{field}");
        }
    }
}
