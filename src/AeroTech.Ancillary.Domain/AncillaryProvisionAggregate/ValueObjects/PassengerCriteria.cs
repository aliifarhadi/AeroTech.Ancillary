using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class PassengerCriteria : ValueObject
    {
        private List<PassengerTypeCode> _passengerTypeCodes = new();

        private PassengerCriteria()
        {
        }

        private PassengerCriteria(IReadOnlyList<PassengerTypeCode> passengerTypeCodes)
            => _passengerTypeCodes = passengerTypeCodes.ToList();

        public IReadOnlyList<PassengerTypeCode> PassengerTypeCodes => _passengerTypeCodes.AsReadOnly();

        public static PassengerCriteria Create(IReadOnlyList<PassengerTypeCode>? passengerTypeCodes)
        {
            var codes = passengerTypeCodes ?? [];

            if (codes.Any(code => !Enum.IsDefined(code)) || codes.Distinct().Count() != codes.Count)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(PassengerCriteria)}.{nameof(PassengerTypeCodes)}");

            return new PassengerCriteria(codes);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return string.Join(',', _passengerTypeCodes);
        }
    }
}
