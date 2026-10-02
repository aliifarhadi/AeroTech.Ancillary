using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto
{
    public sealed record BackofficeAncillaryProductDto(
        long Id,
        int OwnerAirlineId,
        string ProductRef,
        int Version,
        EnumValueDto Type,
        string Name,
        string? Description,
        EnumValueDto SalesScope,
        ProductQuantityDto Quantity,
        ProductDocumentDto Document,
        ProductCodesDto Codes,
        ProductTermsDto Terms,
        EnumValueDto InventoryControl,
        ProductBaggageDto? Baggage,
        EnumValueDto Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? RetiredAt);

    public sealed record ProductQuantityDto(
        EnumValueDto Unit,
        int Min,
        int Max);

    public sealed record ProductDocumentDto(
        EnumValueDto Type,
        string? Rfisc,
        string? Rfic);

    public sealed record ProductCodesDto(
        string ServiceTypeCode,
        string GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code);

    public sealed record ProductTermsDto(
        bool Refundable,
        bool? Commissionable,
        bool? Reusable,
        string? FormOfRefundCode,
        bool? InterlineSettlementAllowed);

    public sealed record ProductBaggageDto(
        int? Pieces,
        decimal? Weight,
        EnumValueDto? WeightUnit);
}
