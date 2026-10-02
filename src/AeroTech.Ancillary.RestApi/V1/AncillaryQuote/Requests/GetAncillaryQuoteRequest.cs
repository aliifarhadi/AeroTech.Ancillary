using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryQuote.Requests
{
    public sealed record GetAncillaryQuoteRequest(
        int CurrencyId,
        DateTimeOffset AsOf,
        QuoteSalesContext? SalesContext,
        IReadOnlyList<QuoteTraveller> Travellers,
        IReadOnlyList<QuoteBound> Bounds,
        IReadOnlyList<QuoteSelection>? Selections,
        IReadOnlyList<QuoteExistingOccurrence>? Existing);
}
