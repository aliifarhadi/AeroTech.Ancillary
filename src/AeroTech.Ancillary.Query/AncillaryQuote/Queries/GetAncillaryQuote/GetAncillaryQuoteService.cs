using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryQuote;
using AeroTech.Ancillary.Query.AncillaryQuote.Dto;

namespace AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote
{
    public sealed class GetAncillaryQuoteService : IGetAncillaryQuoteService
    {
        private readonly IAncillaryProductRepository _products;
        private readonly IAncillaryPriceRuleRepository _priceRules;

        public GetAncillaryQuoteService(IAncillaryProductRepository products, IAncillaryPriceRuleRepository priceRules)
        {
            _products = products;
            _priceRules = priceRules;
        }

        public async Task<AncillaryQuoteDto> ExecuteAsync(IGetAncillaryQuoteQuery query, CancellationToken cancellationToken = default)
        {
            var products = await _products.ListActiveAsync(cancellationToken);
            var priceRules = await _priceRules.ListActiveAsync(query.CurrencyId, cancellationToken);

            var result = AncillaryQuoteEvaluator.Evaluate(ToRequest(query), products, priceRules);

            return new AncillaryQuoteDto(result.CurrencyId, result.AsOf, result.Items.Select(ToDto).ToList());
        }

        private static AncillaryQuoteRequest ToRequest(IGetAncillaryQuoteQuery query)
            => new(
                query.CurrencyId,
                query.AsOf,
                query.Travellers
                    .Select(traveller => new AncillaryQuoteTraveller(traveller.Ref, traveller.PassengerTypeCode, traveller.FlightRefs))
                    .ToList(),
                query.Bounds
                    .Select(bound => new AncillaryQuoteBound(
                        bound.Ref,
                        bound.Flights
                            .Select(flight => new AncillaryQuoteFlight(
                                flight.Ref,
                                flight.OriginAirportId,
                                flight.DestinationAirportId,
                                flight.DepartureDateTime,
                                flight.MarketingAirlineId))
                            .ToList()))
                    .ToList(),
                (query.Existing ?? [])
                    .Select(existing => new AncillaryQuoteExistingOccurrence(
                        existing.ProductRef,
                        existing.TravellerRef,
                        existing.BoundRef,
                        existing.FlightRef,
                        existing.Quantity))
                    .ToList(),
                (query.Selections ?? [])
                    .Select(selection => new AncillaryQuoteSelection(
                        selection.ProductRef,
                        selection.ProductVersion,
                        selection.PriceRuleId,
                        selection.TravellerRef,
                        selection.BoundRef,
                        selection.FlightRef,
                        selection.Quantity))
                    .ToList());

        private static AncillaryQuoteItemDto ToDto(AncillaryQuoteItem item)
        {
            var product = item.Product;

            return new AncillaryQuoteItemDto(
                product.OwnerAirlineId,
                product.ProductRef,
                product.Version,
                product.Type,
                product.Name,
                product.Description,
                product.SalesScope,
                item.TravellerRef,
                item.BoundRef,
                item.FlightRef,
                item.CoveredFlightRefs,
                product.Quantity.Unit,
                product.Quantity.Min,
                item.MaxQuantity,
                item.Quantity,
                new QuoteCodesDto(
                    product.Codes.ServiceTypeCode,
                    product.Codes.GroupCode,
                    product.Codes.SubGroupCode,
                    product.Codes.Description1Code,
                    product.Codes.Description2Code),
                product.Baggage is null
                    ? null
                    : new QuoteBaggageDto(product.Baggage.Pieces, product.Baggage.Weight, product.Baggage.WeightUnit),
                product.Lounge is null
                    ? null
                    : new QuoteLoungeDto(product.Lounge.AirportIds),
                new QuoteTermsDto(
                    product.Terms.Refundable,
                    product.Terms.Commissionable,
                    product.Terms.Reusable,
                    product.Terms.FormOfRefundCode,
                    product.Terms.InterlineSettlementAllowed),
                new QuoteDocumentDto(product.Document.Type, product.Document.Rfic, product.Document.Rfisc),
                new QuoteInventoryDto(product.InventoryControl, null),
                item.PriceRuleId,
                item.PriceLines
                    .Select(line => new QuotePriceLineDto(line.Category, line.Code, line.Name, line.UnitAmount, line.Amount))
                    .ToList(),
                item.UnitTotal,
                item.Total);
        }
    }
}
