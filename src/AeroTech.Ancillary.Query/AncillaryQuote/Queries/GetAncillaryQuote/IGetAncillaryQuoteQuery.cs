namespace AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote
{
    public interface IGetAncillaryQuoteQuery
    {
        int CurrencyId { get; }

        DateTimeOffset AsOf { get; }

        QuoteSalesContext? SalesContext { get; }

        IReadOnlyList<QuoteTraveller> Travellers { get; }

        IReadOnlyList<QuoteBound> Bounds { get; }

        IReadOnlyList<QuoteSelection>? Selections { get; }

        IReadOnlyList<QuoteExistingOccurrence>? Existing { get; }
    }
}
