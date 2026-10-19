using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record FlightOffer(
        string OfferId,
        DateTimeOffset PricedAt,
        DateTimeOffset? LastTicketingDate,
        long? CustomerId,
        int CurrencyId,
        string? CurrencyCode,
        decimal BaseAmount,
        decimal ChargeAmount,
        decimal TotalAmount,
        IReadOnlyList<FlightOfferBound> Bounds,
        IReadOnlyList<FlightOfferFareComponent> FareComponents,
        IReadOnlyList<FlightOfferTicket> Tickets,
        IReadOnlyList<FlightOfferPriceLine> OrderCharges,
        IReadOnlyList<FlightOfferRateOfExchange> RatesOfExchange,
        Channel? SalesChannel,
        long? SalesCountryId);
}
