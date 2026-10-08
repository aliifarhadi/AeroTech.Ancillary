using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionSeasonalPeriod : Entity<long>
    {
        private ProvisionSeasonalPeriod()
        {
        }

        internal ProvisionSeasonalPeriod(long id, long ancillaryProvisionId, ProvisionDatePeriodArgs args)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(args);
        }

        public long AncillaryProvisionId { get; private set; }

        public DateOnly StartDate { get; private set; }

        public DateOnly EndDate { get; private set; }

        internal void Change(ProvisionDatePeriodArgs args)
        {
            Require(args.StartDate != DateOnly.MinValue, nameof(StartDate));
            Require(args.StartDate <= args.EndDate, nameof(EndDate));

            StartDate = args.StartDate;
            EndDate = args.EndDate;
        }

        internal bool SameAs(ProvisionSeasonalPeriod other) => StartDate == other.StartDate && EndDate == other.EndDate;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionSeasonalPeriod)}.{field}");
        }
    }
}
