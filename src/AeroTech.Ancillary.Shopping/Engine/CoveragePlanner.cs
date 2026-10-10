using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal static class CoveragePlanner
    {
        public const string JourneyRef = "JOURNEY";
        public const string OrderRef = "ORDER";

        public static IReadOnlyList<EvaluationUnit> Units(AncillaryShoppingContext context, ShoppingTraveller traveller, ServiceCoverageScope scope)
        {
            var flights = context.Flights.ToDictionary(flight => flight.FlightRef, StringComparer.Ordinal);
            var portions = context.Portions.OrderBy(portion => portion.Sequence).ToList();
            var all = portions.SelectMany(portion => portion.FlightRefs.Select(flightRef => flights[flightRef])).ToList();

            return scope switch
            {
                ServiceCoverageScope.Sector => portions
                    .SelectMany(portion => portion.FlightRefs.Select(flightRef => new EvaluationUnit(context, traveller, scope, flightRef, [flights[flightRef]], [portion], null)))
                    .ToList(),
                ServiceCoverageScope.Portion => portions
                    .Select(portion => new EvaluationUnit(
                        context,
                        traveller,
                        scope,
                        portion.PortionRef,
                        portion.FlightRefs.Select(flightRef => flights[flightRef]).ToList(),
                        [portion],
                        null))
                    .ToList(),
                ServiceCoverageScope.Journey => [new EvaluationUnit(context, traveller, scope, JourneyRef, all, portions, null)],
                ServiceCoverageScope.Order when context.SourceIdentity is { SourceKind: ShoppingSourceKind.Order, OrderId: > 0 }
                    => [new EvaluationUnit(context, traveller, scope, OrderRef, all, portions, null)],
                _ => []
            };
        }
    }
}
