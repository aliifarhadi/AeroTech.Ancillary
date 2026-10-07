using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class SeatApplication : ValueObject
    {
        private const int SeatNumberMaxLength = 16;
        private const int SeatCharacteristicCodeMaxLength = 25;

        private List<string> _seatNumbers = new();
        private List<string> _seatCharacteristicCodes = new();

        private SeatApplication()
        {
        }

        public IReadOnlyList<string> SeatNumbers => _seatNumbers.AsReadOnly();

        public IReadOnlyList<string> SeatCharacteristicCodes => _seatCharacteristicCodes.AsReadOnly();

        public static SeatApplication Create(
            IReadOnlyList<string>? seatNumbers,
            IReadOnlyList<string>? seatCharacteristicCodes)
        {
            var numbers = (seatNumbers ?? []).Select(number => (number ?? string.Empty).Trim().ToUpperInvariant()).ToList();
            var characteristics = (seatCharacteristicCodes ?? []).Select(code => (code ?? string.Empty).Trim()).ToList();

            Require(AreCodes(numbers, SeatNumberMaxLength), nameof(SeatNumbers));
            Require(AreCodes(characteristics, SeatCharacteristicCodeMaxLength), nameof(SeatCharacteristicCodes));
            Require(numbers.Count > 0 || characteristics.Count > 0, nameof(SeatNumbers));

            return new SeatApplication
            {
                _seatNumbers = numbers,
                _seatCharacteristicCodes = characteristics
            };
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return string.Join(',', _seatNumbers);
            yield return string.Join(',', _seatCharacteristicCodes);
        }

        private static bool AreCodes(IReadOnlyList<string> values, int maxLength)
            => values.All(value => value.Length >= 1 && value.Length <= maxLength && !value.Any(char.IsWhiteSpace))
               && values.Distinct(StringComparer.Ordinal).Count() == values.Count;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(SeatApplication)}.{field}");
        }
    }
}
