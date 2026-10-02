using AeroTech.Ancillary.Query.AncillaryQuote.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote.Service
{
    public sealed record ServiceGetAncillaryQuoteQuery(
        int CurrencyId,
        DateTimeOffset AsOf,
        QuoteSalesContext? SalesContext,
        IReadOnlyList<QuoteTraveller> Travellers,
        IReadOnlyList<QuoteBound> Bounds,
        IReadOnlyList<QuoteSelection>? Selections,
        IReadOnlyList<QuoteExistingOccurrence>? Existing) : IRequest<AncillaryQuoteDto>, IGetAncillaryQuoteQuery;
}
