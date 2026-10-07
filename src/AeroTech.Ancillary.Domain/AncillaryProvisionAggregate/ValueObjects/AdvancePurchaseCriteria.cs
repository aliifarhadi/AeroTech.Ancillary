using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class AdvancePurchaseCriteria : ValueObject
    {
        private AdvancePurchaseCriteria()
        {
        }

        private AdvancePurchaseCriteria(int period, TimeUnit unit)
        {
            Period = period;
            Unit = unit;
        }

        public int Period { get; private set; }

        public TimeUnit Unit { get; private set; }

        public static AdvancePurchaseCriteria Create(int period, TimeUnit unit)
        {
            Require(period > 0, nameof(Period));
            Require(Enum.IsDefined(unit), nameof(Unit));

            return new AdvancePurchaseCriteria(period, unit);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Period;
            yield return Unit;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(AdvancePurchaseCriteria)}.{field}");
        }
    }
}
