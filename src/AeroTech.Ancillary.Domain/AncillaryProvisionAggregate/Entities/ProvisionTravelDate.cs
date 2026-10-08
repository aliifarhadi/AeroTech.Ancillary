using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionTravelDate : Entity<long>
    {
        private ProvisionTravelDate()
        {
        }

        internal ProvisionTravelDate(long id, long ancillaryProvisionId, DateOnly travelDate)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(travelDate);
        }

        public long AncillaryProvisionId { get; private set; }

        public DateOnly TravelDate { get; private set; }

        internal void Change(DateOnly travelDate)
        {
            if (travelDate == DateOnly.MinValue)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionTravelDate)}.{nameof(TravelDate)}");

            TravelDate = travelDate;
        }

        internal bool SameAs(ProvisionTravelDate other) => TravelDate == other.TravelDate;
    }
}
