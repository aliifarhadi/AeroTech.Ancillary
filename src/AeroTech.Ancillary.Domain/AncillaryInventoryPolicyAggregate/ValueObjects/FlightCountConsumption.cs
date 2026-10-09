using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects
{
    public sealed class FlightCountConsumption : ValueObject
    {
        private FlightCountConsumption()
        {
        }

        private FlightCountConsumption(long resourceId, int countPerAcceptedUnit, InventoryCountUnit countUnit)
        {
            ResourceId = resourceId;
            CountPerAcceptedUnit = countPerAcceptedUnit;
            CountUnit = countUnit;
        }

        public long ResourceId { get; private set; }

        public int CountPerAcceptedUnit { get; private set; }

        public InventoryCountUnit CountUnit { get; private set; }

        public static FlightCountConsumption Create(long resourceId, int countPerAcceptedUnit, InventoryCountUnit countUnit)
        {
            Require(resourceId > 0, nameof(ResourceId));
            Require(countPerAcceptedUnit > 0, nameof(CountPerAcceptedUnit));
            Require(Enum.IsDefined(countUnit), nameof(CountUnit));

            return new FlightCountConsumption(resourceId, countPerAcceptedUnit, countUnit);
        }

        public int RequiredUnits(int acceptedQuantity)
        {
            Require(acceptedQuantity > 0, nameof(acceptedQuantity));

            var required = (long)acceptedQuantity * CountPerAcceptedUnit;

            Require(required <= int.MaxValue, nameof(acceptedQuantity));

            return (int)required;
        }

        internal FlightCountConsumption Copy() => new(ResourceId, CountPerAcceptedUnit, CountUnit);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return ResourceId;
            yield return CountPerAcceptedUnit;
            yield return CountUnit;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.InventoryPolicyIsInvalid($"{nameof(FlightCountConsumption)}.{field}");
        }
    }
}
