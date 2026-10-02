using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct
{
    public sealed record ProductQuantity(
        AncillaryQuantityUnit Unit,
        int Min,
        int Max);

    public sealed record ProductDocument(
        AncillaryDocumentType Type,
        string? Rfisc);

    public sealed record ProductCodes(
        string ServiceTypeCode);

    public sealed record ProductTerms(
        bool Refundable,
        bool? Commissionable,
        bool? Reusable,
        string? FormOfRefundCode,
        bool? InterlineSettlementAllowed);

    public sealed record ProductBaggage(
        int? Pieces,
        decimal? Weight,
        AncillaryWeightUnit? WeightUnit);

    public sealed record ProductLounge(
        IReadOnlyList<int> AirportIds);
}
