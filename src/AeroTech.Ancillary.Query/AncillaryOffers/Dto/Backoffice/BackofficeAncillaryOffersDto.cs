using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Dto
{
    public sealed record AncillaryOffersDto(
        string OfferId,
        DateTimeOffset EvaluatedAtUtc,
        OfferCurrencyDto Currency,
        FlightOfferSummaryDto FlightOffer,
        OfferScopeDto Scope,
        OfferJourneyDto Journey,
        IReadOnlyList<OfferTravellerDto> Travellers,
        IReadOnlyList<OfferCategoryDto> Categories,
        IReadOnlyList<AncillaryOfferDto> Offers,
        IReadOnlyList<AncillaryOfferDto> BlockedOffers,
        IReadOnlyList<OfferDiagnosticDto> Diagnostics,
        OfferCompletenessDto Completeness);

    public sealed record OfferCurrencyDto(int CurrencyId, string? CurrencyCode);

    public sealed record FlightOfferSummaryDto(
        DateTimeOffset? LastTicketingDate,
        DateTimeOffset? SourcePricedAt,
        int CurrencyId,
        decimal BaseAmount,
        decimal ChargeAmount,
        decimal TotalAmount);

    public sealed record OfferScopeDto(int OwnerAirlineId, long PointOfSaleId, EnumValueDto ShoppingStage);

    public sealed record OfferCategoryDto(EnumValueDto Code, string Label, int OfferCount);

    public sealed record OfferDiagnosticDto(string Code, EnumValueDto Severity, IReadOnlyList<string> MissingFacts, string? Source);

    public sealed record OfferCompletenessDto(
        bool FlightOfferVerified,
        bool ShoppingContextComplete,
        bool HasBlockedProducts,
        bool LiveSupplierAvailabilityChecked);
}
