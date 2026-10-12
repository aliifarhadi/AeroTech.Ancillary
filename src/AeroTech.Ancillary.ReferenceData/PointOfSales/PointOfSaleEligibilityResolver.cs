using AeroTech.Ancillary.ReferenceData.ReadModels;

namespace AeroTech.Ancillary.ReferenceData.PointOfSales
{
    public static class PointOfSaleEligibilityResolver
    {
        public static HashSet<long> ResolveAllowedPointOfSaleIds(
            IEnumerable<long> pointOfSaleIds,
            IEnumerable<PointOfSaleConditionReadModel> conditions,
            long? countryId,
            IReadOnlyList<long> airlineOfficeIds,
            long? travelAgencyOfficeId,
            string channel)
        {
            var conditionsByPointOfSale = conditions
                .GroupBy(condition => condition.PointOfSaleId)
                .ToDictionary(group => group.Key, group => group.ToList());
            var allowedPointOfSaleIds = new HashSet<long>();

            foreach (var pointOfSaleId in pointOfSaleIds)
            {
                if (!conditionsByPointOfSale.TryGetValue(pointOfSaleId, out var own) || own.Count == 0)
                    continue;

                if (own.Any(condition => ConditionMatches(condition, countryId, airlineOfficeIds, travelAgencyOfficeId, channel)))
                    allowedPointOfSaleIds.Add(pointOfSaleId);
            }

            return allowedPointOfSaleIds;
        }

        private static bool ConditionMatches(
            PointOfSaleConditionReadModel condition,
            long? countryId,
            IReadOnlyList<long> airlineOfficeIds,
            long? travelAgencyOfficeId,
            string channel)
            => OptionalDimensionMatches(countryId, condition.CountryIds.Select(id => (long)id).ToHashSet(), condition.CountryIsInclude)
               && DimensionMatchesAny(airlineOfficeIds, condition.AirlineOfficeIds.ToHashSet(), condition.AirlineOfficeIsInclude)
               && DimensionMatches(travelAgencyOfficeId, condition.TravelAgencyOfficeIds.ToHashSet(), condition.TravelAgencyOfficeIsInclude)
               && ChannelMatches(channel, condition.Channels, condition.ChannelIsInclude);

        private static bool DimensionMatchesAny(IReadOnlyList<long> requestedValues, HashSet<long> conditionValues, bool? isInclude)
        {
            if (conditionValues.Count == 0)
                return true;

            var contains = requestedValues.Any(conditionValues.Contains);

            return isInclude != false ? contains : !contains;
        }

        private static bool OptionalDimensionMatches(long? requestedValue, HashSet<long> conditionValues, bool? isInclude)
            => !requestedValue.HasValue || DimensionMatches(requestedValue, conditionValues, isInclude);

        private static bool DimensionMatches(long? requestedValue, HashSet<long> conditionValues, bool? isInclude)
        {
            if (conditionValues.Count == 0)
                return true;

            var contains = requestedValue.HasValue && conditionValues.Contains(requestedValue.Value);

            return isInclude != false ? contains : !contains;
        }

        private static bool ChannelMatches(string channel, List<string> conditionChannels, bool? isInclude)
        {
            if (conditionChannels.Count == 0)
                return true;

            var contains = conditionChannels.Contains(channel, StringComparer.Ordinal);

            return isInclude != false ? contains : !contains;
        }
    }
}
