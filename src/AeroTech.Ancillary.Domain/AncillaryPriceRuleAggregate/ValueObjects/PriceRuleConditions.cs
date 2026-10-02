using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.ValueObjects
{
    public sealed class PriceRuleConditions : ValueObject
    {
        private List<PassengerTypeCode>? _passengerTypes;
        private List<int>? _originAirportIds;
        private List<int>? _destinationAirportIds;

        private PriceRuleConditions()
        {
        }

        public PriceRuleConditions(
            IReadOnlyCollection<PassengerTypeCode>? passengerTypes,
            IReadOnlyCollection<int>? originAirportIds,
            IReadOnlyCollection<int>? destinationAirportIds)
        {
            if (!IsRestriction(passengerTypes) || passengerTypes?.All(passengerType => Enum.IsDefined(passengerType)) == false)
                throw ExceptionFactory.AncillaryPriceRuleIsInvalid($"{nameof(PriceRuleConditions)}.{nameof(PassengerTypes)}");

            if (!IsRestriction(originAirportIds))
                throw ExceptionFactory.AncillaryPriceRuleIsInvalid($"{nameof(PriceRuleConditions)}.{nameof(OriginAirportIds)}");

            if (!IsRestriction(destinationAirportIds))
                throw ExceptionFactory.AncillaryPriceRuleIsInvalid($"{nameof(PriceRuleConditions)}.{nameof(DestinationAirportIds)}");

            _passengerTypes = passengerTypes?.ToList();
            _originAirportIds = originAirportIds?.ToList();
            _destinationAirportIds = destinationAirportIds?.ToList();
        }

        public IReadOnlyCollection<PassengerTypeCode>? PassengerTypes => _passengerTypes?.AsReadOnly();

        public IReadOnlyCollection<int>? OriginAirportIds => _originAirportIds?.AsReadOnly();

        public IReadOnlyCollection<int>? DestinationAirportIds => _destinationAirportIds?.AsReadOnly();

        public bool Match(PassengerTypeCode passengerType, int originAirportId, int destinationAirportId)
            => (_passengerTypes is null || _passengerTypes.Contains(passengerType))
               && (_originAirportIds is null || _originAirportIds.Contains(originAirportId))
               && (_destinationAirportIds is null || _destinationAirportIds.Contains(destinationAirportId));

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return _passengerTypes is null ? null : string.Join(',', _passengerTypes.Order());
            yield return _originAirportIds is null ? null : string.Join(',', _originAirportIds.Order());
            yield return _destinationAirportIds is null ? null : string.Join(',', _destinationAirportIds.Order());
        }

        private static bool IsRestriction<T>(IReadOnlyCollection<T>? values)
            => values is null || (values.Count > 0 && values.Distinct().Count() == values.Count);
    }
}
