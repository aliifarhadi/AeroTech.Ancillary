using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate;

namespace AeroTech.Ancillary.Domain.AncillaryQuote
{
    internal sealed record AncillaryQuoteOccurrence(
        AncillaryProduct Product,
        AncillaryQuoteTraveller Traveller,
        AncillaryQuoteBound Bound,
        AncillaryQuoteFlight? Flight,
        IReadOnlyList<AncillaryQuoteFlight> CoveredFlights,
        AncillaryPriceRule PriceRule,
        int Remaining);
}
