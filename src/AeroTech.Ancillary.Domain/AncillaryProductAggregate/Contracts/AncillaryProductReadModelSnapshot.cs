using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts
{
    public sealed record AncillaryProductReadModelSnapshot(
        long AncillaryProductId,
        int OwnerAirlineId,
        string ProductRef,
        int Version,
        AncillaryProductType Type,
        string Name,
        string? Description,
        AncillarySalesScope SalesScope,
        AncillaryQuantityUnit QuantityUnit,
        int QuantityMin,
        int QuantityMax,
        AncillaryDocumentType DocumentType,
        string? Rfisc,
        string? Rfic,
        string ServiceTypeCode,
        string GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        bool Refundable,
        bool? Commissionable,
        bool? Reusable,
        string? FormOfRefundCode,
        bool? InterlineSettlementAllowed,
        AncillaryInventoryControl InventoryControl,
        int? BaggagePieces,
        decimal? BaggageWeight,
        AncillaryWeightUnit? BaggageWeightUnit,
        IReadOnlyCollection<int>? LoungeAirportIds,
        AncillaryProductStatus Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? RetiredAt);
}
