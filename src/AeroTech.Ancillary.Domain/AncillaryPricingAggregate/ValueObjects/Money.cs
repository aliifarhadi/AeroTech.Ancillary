using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;

namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate.ValueObjects
{
    public sealed class Money : ValueObject
    {
        private const int StoredScale = 6;
        private const decimal MaxAmount = 9_999_999_999_999.999999m;

        private Money()
        {
        }

        private Money(decimal amount, int currencyId)
        {
            Amount = amount;
            CurrencyId = currencyId;
        }

        public decimal Amount { get; private set; }

        public int CurrencyId { get; private set; }

        public static Money Of(decimal amount, int currencyId)
        {
            Require(currencyId > 0, nameof(CurrencyId));
            Require(amount >= 0m && amount <= MaxAmount && ScaleOf(amount) <= StoredScale, nameof(Amount));

            return new Money(amount, currencyId);
        }

        public Money Add(Money other)
        {
            if (other.CurrencyId != CurrencyId)
                throw ExceptionFactory.PricingCurrencyMismatch(CurrencyId, other.CurrencyId);

            return Of(Amount + other.Amount, CurrencyId);
        }

        internal void EnsureScale(IReadOnlyDictionary<int, int> currencyDecimalPlaces)
        {
            if (!currencyDecimalPlaces.TryGetValue(CurrencyId, out var decimalPlaces))
                throw ExceptionFactory.PricingCurrencyNotFound(CurrencyId);

            if (decimalPlaces < 0 || ScaleOf(Amount) > decimalPlaces)
                throw ExceptionFactory.PricingAmountScaleNotAllowed(Amount, CurrencyId, decimalPlaces);
        }

        internal Money Copy() => new(Amount, CurrencyId);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return CurrencyId;
        }

        private static int ScaleOf(decimal amount)
        {
            var scale = amount.Scale;

            while (scale > 0 && decimal.Round(amount, scale - 1) == amount)
                scale--;

            return scale;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.PricingIsInvalid($"{nameof(Money)}.{field}");
        }
    }
}
