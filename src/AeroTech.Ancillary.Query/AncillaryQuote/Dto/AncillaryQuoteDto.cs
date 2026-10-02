using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryQuote.Dto
{
    public sealed record AncillaryQuoteDto(
        int CurrencyId,
        DateTimeOffset AsOf,
        IReadOnlyList<AncillaryQuoteItemDto> Items);

    public sealed record AncillaryQuoteItemDto(
        int OwnerAirlineId,
        string ProductRef,
        int ProductVersion,
        AncillaryProductType Type,
        string Name,
        string? Description,
        AncillarySalesScope SalesScope,
        string TravellerRef,
        string BoundRef,
        string? FlightRef,
        IReadOnlyList<string> CoveredFlightRefs,
        AncillaryQuantityUnit Unit,
        int MinQuantity,
        int MaxQuantity,
        int Quantity,
        QuoteCodesDto Codes,
        QuoteBaggageDto? Baggage,
        QuoteLoungeDto? Lounge,
        QuoteTermsDto Terms,
        QuoteDocumentDto Document,
        QuoteInventoryDto Inventory,
        long PriceRuleId,
        IReadOnlyList<QuotePriceLineDto> PriceLines,
        decimal UnitTotal,
        decimal Total);

    public sealed record QuoteCodesDto(
        string ServiceTypeCode,
        string GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code);

    public sealed record QuoteBaggageDto(
        int? Pieces,
        decimal? Weight,
        AncillaryWeightUnit? WeightUnit);

    public sealed record QuoteLoungeDto(
        IReadOnlyCollection<int> AirportIds);

    public sealed record QuoteTermsDto(
        bool Refundable,
        bool? Commissionable,
        bool? Reusable,
        string? FormOfRefundCode,
        bool? InterlineSettlementAllowed);

    public sealed record QuoteDocumentDto(
        AncillaryDocumentType Type,
        string? Rfic,
        string? Rfisc);

    public sealed record QuoteInventoryDto(
        AncillaryInventoryControl Control,
        int? Remaining);

    public sealed record QuotePriceLineDto(
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        decimal UnitAmount,
        decimal Amount);
}
