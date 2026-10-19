using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal static class CoveragePlanner
    {
        public const string JourneyRef = "JOURNEY";
        public const string OrderRef = "ORDER";

        public static IReadOnlyList<EvaluationUnit> Units(AncillaryShoppingContext context, ShoppingTraveller? traveller, ServiceCoverageScope scope)
        {
            var flights = context.Flights.ToDictionary(flight => flight.FlightRef, StringComparer.Ordinal);
            var portions = context.Portions.OrderBy(portion => portion.Sequence).ToList();
            var all = portions.SelectMany(portion => portion.FlightRefs.Select(flightRef => flights[flightRef])).ToList();

            return scope switch
            {
                ServiceCoverageScope.Sector when traveller is not null => portions
                    .SelectMany(portion => portion.FlightRefs.Select(flightRef => new EvaluationUnit(context, traveller, scope, flightRef, [flights[flightRef]], [portion], null)))
                    .ToList(),
                ServiceCoverageScope.Portion when traveller is not null => portions
                    .Select(portion => new EvaluationUnit(
                        context,
                        traveller,
                        scope,
                        portion.PortionRef,
                        portion.FlightRefs.Select(flightRef => flights[flightRef]).ToList(),
                        [portion],
                        null))
                    .ToList(),
                ServiceCoverageScope.Journey when traveller is not null => [new EvaluationUnit(context, traveller, scope, JourneyRef, all, portions, null)],
                ServiceCoverageScope.Order when OrderCoverable(context.SourceIdentity)
                    => [new EvaluationUnit(context, null, scope, OrderRef, all, portions, null)],
                _ => []
            };
        }

        public static bool AllowsOrderScope(AncillaryServiceDefinition definition)
            => definition.VariantCode is AncillaryVariant.CipMeetAssist or AncillaryVariant.OnboardWifi;

        private static bool OrderCoverable(SourceIdentityContext source)
            => source.SourceKind == ShoppingSourceKind.Offer
               || source is { SourceKind: ShoppingSourceKind.Order, OrderId: > 0 };
    }
}
