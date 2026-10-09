using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Domain._Shared.Rules;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects
{
    public sealed class FlightWeightConsumption : ValueObject
    {
        private FlightWeightConsumption()
        {
        }

        private FlightWeightConsumption(long weightResourceId, FlightWeightConsumptionMode consumptionMode, decimal? fixedKgPerUnit)
        {
            WeightResourceId = weightResourceId;
            ConsumptionMode = consumptionMode;
            FixedKgPerUnit = fixedKgPerUnit;
        }

        public long WeightResourceId { get; private set; }

        public FlightWeightConsumptionMode ConsumptionMode { get; private set; }

        public decimal? FixedKgPerUnit { get; private set; }

        public static FlightWeightConsumption FixedPerUnit(long weightResourceId, decimal fixedKgPerUnit)
        {
            Require(weightResourceId > 0, nameof(WeightResourceId));
            Require(fixedKgPerUnit > 0m && InventoryRules.IsKg(fixedKgPerUnit), nameof(FixedKgPerUnit));

            return new FlightWeightConsumption(weightResourceId, FlightWeightConsumptionMode.FixedKgPerAcceptedUnit, fixedKgPerUnit);
        }

        public static FlightWeightConsumption AcceptedWeight(long weightResourceId)
        {
            Require(weightResourceId > 0, nameof(WeightResourceId));

            return new FlightWeightConsumption(weightResourceId, FlightWeightConsumptionMode.AcceptedWeightKg, null);
        }

        public decimal RequiredKg(int acceptedQuantity, decimal? acceptedWeightKg)
        {
            if (ConsumptionMode == FlightWeightConsumptionMode.AcceptedWeightKg)
            {
                Require(acceptedWeightKg is > 0m && InventoryRules.IsKg(acceptedWeightKg.Value), nameof(acceptedWeightKg));

                return acceptedWeightKg!.Value;
            }

            Require(acceptedQuantity > 0, nameof(acceptedQuantity));
            Require(FixedKgPerUnit!.Value <= InventoryRules.MaxKg / acceptedQuantity, nameof(acceptedQuantity));

            return acceptedQuantity * FixedKgPerUnit.Value;
        }

        internal FlightWeightConsumption Copy() => new(WeightResourceId, ConsumptionMode, FixedKgPerUnit);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return WeightResourceId;
            yield return ConsumptionMode;
            yield return FixedKgPerUnit;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.InventoryPolicyIsInvalid($"{nameof(FlightWeightConsumption)}.{field}");
        }
    }
}
