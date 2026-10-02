using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryQuote
{
    public static class AncillaryQuoteEvaluator
    {
        public static AncillaryQuoteResult Evaluate(
            AncillaryQuoteRequest request,
            IReadOnlyCollection<AncillaryProduct> products,
            IReadOnlyCollection<AncillaryPriceRule> priceRules)
        {
            var context = AncillaryQuoteContext.Of(request);
            var activeProducts = products
                .Where(product => product.Status == AncillaryProductStatus.Active)
                .OrderBy(product => product.ProductRef, StringComparer.Ordinal)
                .ToList();
            var activeRules = priceRules.Where(rule => rule.Status == AncillaryPriceRuleStatus.Active).ToList();

            var items = request.Selections.Count == 0
                ? Catalogue(request, context, activeProducts, activeRules)
                : Selected(request, context, activeProducts, activeRules);

            return new AncillaryQuoteResult(request.CurrencyId, request.AsOf, items);
        }

        private static List<AncillaryQuoteItem> Catalogue(
            AncillaryQuoteRequest request,
            AncillaryQuoteContext context,
            IReadOnlyList<AncillaryProduct> products,
            IReadOnlyList<AncillaryPriceRule> priceRules)
        {
            var items = new List<AncillaryQuoteItem>();
            var boundScoped = products.Where(product => product.SalesScope == AncillarySalesScope.TravellerBound).ToList();
            var flightScoped = products.Where(product => product.SalesScope == AncillarySalesScope.TravellerSegment).ToList();

            foreach (var traveller in request.Travellers)
            {
                foreach (var bound in request.Bounds)
                {
                    items.AddRange(CatalogueItems(request, context, priceRules, boundScoped, traveller, bound, null));

                    foreach (var flight in bound.Flights)
                        items.AddRange(CatalogueItems(request, context, priceRules, flightScoped, traveller, bound, flight));
                }
            }

            return items;
        }

        private static IEnumerable<AncillaryQuoteItem> CatalogueItems(
            AncillaryQuoteRequest request,
            AncillaryQuoteContext context,
            IReadOnlyList<AncillaryPriceRule> priceRules,
            IReadOnlyList<AncillaryProduct> products,
            AncillaryQuoteTraveller traveller,
            AncillaryQuoteBound bound,
            AncillaryQuoteFlight? flight)
            => products
                .Select(product => Occurrence(request, context, priceRules, product, traveller, bound, flight))
                .OfType<AncillaryQuoteOccurrence>()
                .Select(occurrence => Item(occurrence, occurrence.Product.Quantity.Min));

        private static List<AncillaryQuoteItem> Selected(
            AncillaryQuoteRequest request,
            AncillaryQuoteContext context,
            IReadOnlyList<AncillaryProduct> products,
            IReadOnlyList<AncillaryPriceRule> priceRules)
        {
            var items = new List<AncillaryQuoteItem>();
            var selected = new HashSet<(string ProductRef, string TravellerRef, string? BoundRef, string? FlightRef)>();

            foreach (var selection in request.Selections)
            {
                var traveller = context.Traveller(selection.TravellerRef);
                var bound = selection.BoundRef is null ? null : context.Bound(selection.BoundRef);
                var flight = selection.FlightRef is null ? null : context.Flight(selection.FlightRef);
                var candidates = products.Where(product => product.ProductRef == selection.ProductRef).ToList();

                if (candidates.Count == 0)
                    throw ExceptionFactory.AncillaryQuoteSelectedProductNotFound();

                var referred = flight ?? (bound is null ? null : context.CoveredFlights(traveller, bound).FirstOrDefault());
                var product = referred is null
                    ? null
                    : candidates.FirstOrDefault(candidate => candidate.OwnerAirlineId == referred.MarketingAirlineId);
                var salesScope = (product ?? candidates[0]).SalesScope;

                if (!FitsScope(salesScope, bound, flight))
                    throw ExceptionFactory.AncillaryQuoteSelectionDoesNotFitScope();

                var occurrenceFlight = salesScope == AncillarySalesScope.TravellerSegment ? flight : null;
                var occurrenceBound = occurrenceFlight is null ? bound! : context.BoundOf(occurrenceFlight);

                if (!selected.Add((selection.ProductRef, selection.TravellerRef, occurrenceFlight is null ? occurrenceBound.Ref : null, occurrenceFlight?.Ref)))
                    throw ExceptionFactory.AncillaryQuoteOccurrenceSelectedTwice();

                var occurrence = product is null
                    ? null
                    : Occurrence(request, context, priceRules, product, traveller, occurrenceBound, occurrenceFlight);

                if (occurrence is null)
                    throw ExceptionFactory.AncillaryQuoteOccurrenceNotApplicable();

                if (selection.ProductVersion != occurrence.Product.Version || selection.PriceRuleId != occurrence.PriceRule.Id)
                    throw ExceptionFactory.AncillaryQuoteSelectionNoLongerCurrent();

                if (selection.Quantity < occurrence.Product.Quantity.Min || selection.Quantity > occurrence.Remaining)
                    throw ExceptionFactory.AncillaryQuoteQuantityNotAllowed();

                items.Add(Item(occurrence, selection.Quantity));
            }

            return items;
        }

        private static bool FitsScope(AncillarySalesScope salesScope, AncillaryQuoteBound? bound, AncillaryQuoteFlight? flight)
            => salesScope switch
            {
                AncillarySalesScope.TravellerBound => bound is not null && flight is null,
                AncillarySalesScope.TravellerSegment => flight is not null && (bound is null || bound.Flights.Contains(flight)),
                _ => false
            };

        private static AncillaryQuoteOccurrence? Occurrence(
            AncillaryQuoteRequest request,
            AncillaryQuoteContext context,
            IReadOnlyList<AncillaryPriceRule> priceRules,
            AncillaryProduct product,
            AncillaryQuoteTraveller traveller,
            AncillaryQuoteBound bound,
            AncillaryQuoteFlight? flight)
        {
            var covered = context.CoveredFlights(traveller, bound)
                .Where(candidate => flight is null || candidate.Ref == flight.Ref)
                .ToList();

            if (covered.Count == 0
                || covered.Any(candidate => candidate.MarketingAirlineId != product.OwnerAirlineId)
                || !IsOfferedOn(product, covered))
                return null;

            var priceRule = PriceRule(request, context, priceRules, product, traveller, covered);

            if (priceRule is null)
                return null;

            var remaining = product.Quantity.Max - context.ExistingQuantity(product.ProductRef, traveller.Ref, bound, flight);

            return remaining < product.Quantity.Min
                ? null
                : new AncillaryQuoteOccurrence(product, traveller, bound, flight, covered, priceRule, remaining);
        }

        private static bool IsOfferedOn(AncillaryProduct product, IReadOnlyList<AncillaryQuoteFlight> covered)
            => product.Type switch
            {
                AncillaryProductType.ExtraBaggage => covered.Count == 1,
                AncillaryProductType.LoungeAccess => product.Lounge!.IsOfferedAt(covered[0].OriginAirportId),
                _ => false
            };

        private static AncillaryPriceRule? PriceRule(
            AncillaryQuoteRequest request,
            AncillaryQuoteContext context,
            IReadOnlyList<AncillaryPriceRule> priceRules,
            AncillaryProduct product,
            AncillaryQuoteTraveller traveller,
            IReadOnlyList<AncillaryQuoteFlight> covered)
        {
            var first = covered[0];
            var last = covered[^1];
            var travelDate = DateOnly.FromDateTime(first.DepartureDateTime.DateTime);
            var passengerType = context.PassengerTypeOf(traveller);

            return priceRules
                .Where(rule => rule.OwnerAirlineId == product.OwnerAirlineId
                               && rule.ProductRef == product.ProductRef
                               && rule.CurrencyId == request.CurrencyId
                               && (rule.SalesFrom is null || rule.SalesFrom <= request.AsOf)
                               && (rule.SalesTo is null || request.AsOf < rule.SalesTo)
                               && (rule.TravelFrom is null || travelDate >= rule.TravelFrom)
                               && (rule.TravelTo is null || travelDate <= rule.TravelTo)
                               && rule.Conditions.Match(passengerType, first.OriginAirportId, last.DestinationAirportId))
                .MinBy(rule => rule.Priority);
        }

        private static AncillaryQuoteItem Item(AncillaryQuoteOccurrence occurrence, int quantity)
        {
            var priceLines = occurrence.PriceRule.Lines
                .OrderBy(line => line.Category == AncillaryPriceLineCategory.Ancillary ? 0 : 1)
                .ThenBy(line => line.Id)
                .Select(line => new AncillaryQuotePriceLine(
                    line.Category,
                    line.Code,
                    line.Name ?? (line.Category == AncillaryPriceLineCategory.Ancillary ? occurrence.Product.Name : null),
                    line.Amount,
                    line.Amount * quantity))
                .ToList();

            return new AncillaryQuoteItem(
                occurrence.Product,
                occurrence.Traveller.Ref,
                occurrence.Bound.Ref,
                occurrence.Flight?.Ref,
                occurrence.CoveredFlights.Select(flight => flight.Ref).ToList(),
                occurrence.Remaining,
                quantity,
                occurrence.PriceRule.Id,
                priceLines,
                priceLines.Sum(line => line.UnitAmount),
                priceLines.Sum(line => line.Amount));
        }
    }
}
