using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public sealed class PointOfSaleEligibilityService : IPointOfSaleEligibilityReference
    {
        private readonly IPointOfSaleConditionReference _pointsOfSale;

        public PointOfSaleEligibilityService(IPointOfSaleConditionReference pointsOfSale) => _pointsOfSale = pointsOfSale;

        public async Task<PointOfSaleEligibility> ResolveAsync(PointOfSaleEligibilityQuery query, CancellationToken cancellationToken = default)
        {
            var conditionSet = await _pointsOfSale.GetConditionSetAsync(cancellationToken);

            return new PointOfSaleEligibility(InventoryReferenceCheck.Verified, ResolveAllowedPointOfSaleIds(conditionSet, query));
        }

        private static HashSet<long> ResolveAllowedPointOfSaleIds(PointOfSaleConditionSet conditionSet, PointOfSaleEligibilityQuery query)
        {
            var conditionsByPointOfSale = conditionSet.Conditions
                .GroupBy(condition => condition.PointOfSaleId)
                .ToDictionary(group => group.Key, group => group.ToList());
            var channel = AirPriceChannel(query.Channel);
            var allowedPointOfSaleIds = new HashSet<long>();

            foreach (var pointOfSaleId in conditionSet.PointOfSaleIds)
            {
                if (!conditionsByPointOfSale.TryGetValue(pointOfSaleId, out var conditions) || conditions.Count == 0)
                    continue;

                if (conditions.Any(condition => ConditionMatches(condition, query, channel)))
                    allowedPointOfSaleIds.Add(pointOfSaleId);
            }

            return allowedPointOfSaleIds;
        }

        private static bool ConditionMatches(PointOfSaleCondition condition, PointOfSaleEligibilityQuery query, string channel)
            => OptionalDimensionMatches(query.CountryId, condition.CountryIds, condition.CountryIsInclude)
               && DimensionMatchesAny(query.AirlineOfficeIds, condition.AirlineOfficeIds, condition.AirlineOfficeIsInclude)
               && DimensionMatches(query.TravelAgencyOfficeId, condition.TravelAgencyOfficeIds, condition.TravelAgencyOfficeIsInclude)
               && ChannelMatches(channel, condition.Channels, condition.ChannelIsInclude);

        private static bool DimensionMatchesAny(IReadOnlyList<long> requestedValues, IReadOnlyCollection<long> conditionValues, bool? isInclude)
        {
            if (conditionValues.Count == 0)
                return true;

            var contains = requestedValues.Any(conditionValues.Contains);

            return isInclude != false ? contains : !contains;
        }

        private static bool OptionalDimensionMatches(long? requestedValue, IReadOnlyCollection<long> conditionValues, bool? isInclude)
            => !requestedValue.HasValue || DimensionMatches(requestedValue, conditionValues, isInclude);

        private static bool DimensionMatches(long? requestedValue, IReadOnlyCollection<long> conditionValues, bool? isInclude)
        {
            if (conditionValues.Count == 0)
                return true;

            var contains = requestedValue.HasValue && conditionValues.Contains(requestedValue.Value);

            return isInclude != false ? contains : !contains;
        }

        private static bool ChannelMatches(string channel, IReadOnlyCollection<string> conditionChannels, bool? isInclude)
        {
            if (conditionChannels.Count == 0)
                return true;

            var contains = conditionChannels.Contains(channel, StringComparer.Ordinal);

            return isInclude != false ? contains : !contains;
        }

        private static string AirPriceChannel(Channel channel) => channel switch
        {
            Channel.PartnerAPI => "API",
            Channel.AgencyPanel => "GUI",
            _ => channel.ToString()
        };
    }
}
