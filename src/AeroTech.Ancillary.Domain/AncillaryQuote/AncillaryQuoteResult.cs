using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryQuote
{
    public sealed record AncillaryQuoteResult(
        int CurrencyId,
        DateTimeOffset AsOf,
        IReadOnlyList<AncillaryQuoteItem> Items);

    public sealed record AncillaryQuoteItem(
        AncillaryProduct Product,
        string TravellerRef,
        string BoundRef,
        IReadOnlyList<string> CoveredFlightRefs,
        int MaxQuantity,
        int Quantity,
        long PriceRuleId,
        IReadOnlyList<AncillaryQuotePriceLine> PriceLines,
        decimal UnitTotal,
        decimal Total);

    public sealed record AncillaryQuotePriceLine(
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        decimal UnitAmount,
        decimal Amount);
}
