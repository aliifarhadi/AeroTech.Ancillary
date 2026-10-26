using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects
{
    public sealed class AirportSlotConsumption : ValueObject
    {
        private AirportSlotConsumption()
        {
        }

        private AirportSlotConsumption(long facilityId, int occupancyMinutes, int peoplePerAcceptedUnit)
        {
            FacilityId = facilityId;
            OccupancyMinutes = occupancyMinutes;
            PeoplePerAcceptedUnit = peoplePerAcceptedUnit;
        }

        public long FacilityId { get; private set; }

        public int OccupancyMinutes { get; private set; }

        public int PeoplePerAcceptedUnit { get; private set; }

        public static AirportSlotConsumption Create(long facilityId, int occupancyMinutes, int peoplePerAcceptedUnit)
        {
            Require(facilityId > 0, nameof(FacilityId));
            Require(occupancyMinutes > 0, nameof(OccupancyMinutes));
            Require(peoplePerAcceptedUnit > 0, nameof(PeoplePerAcceptedUnit));

            return new AirportSlotConsumption(facilityId, occupancyMinutes, peoplePerAcceptedUnit);
        }

        public int RequiredPersons(int acceptedQuantity, int guestCount)
        {
            Require(acceptedQuantity > 0, nameof(acceptedQuantity));
            Require(guestCount >= 0, nameof(guestCount));

            var required = (long)PeoplePerAcceptedUnit * ((long)acceptedQuantity + guestCount);

            Require(required <= int.MaxValue, nameof(acceptedQuantity));

            return (int)required;
        }

        internal AirportSlotConsumption Copy() => new(FacilityId, OccupancyMinutes, PeoplePerAcceptedUnit);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return FacilityId;
            yield return OccupancyMinutes;
            yield return PeoplePerAcceptedUnit;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.InventoryPolicyIsInvalid($"{nameof(AirportSlotConsumption)}.{field}");
        }
    }
}
