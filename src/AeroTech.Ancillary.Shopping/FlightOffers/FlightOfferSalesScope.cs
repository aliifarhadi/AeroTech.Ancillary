namespace AeroTech.Ancillary.Shopping.FlightOffers
{
    public sealed record FlightOfferSalesScope(
        int OwnerAirlineId,
        long PointOfSaleId,
        long? CustomerId,
        int? PreferredCurrencyId,
        DateTimeOffset EvaluatedAtUtc);
}
