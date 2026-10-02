using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Projection
{
    internal static class AncillaryProductReadModelSnapshotFactory
    {
        public static AncillaryProductReadModelSnapshot ToReadModelSnapshot(this AncillaryProduct product)
            => new(
                product.Id,
                product.OwnerAirlineId,
                product.ProductRef,
                product.Version,
                product.Type,
                product.Name,
                product.Description,
                product.SalesScope,
                product.Quantity.Unit,
                product.Quantity.Min,
                product.Quantity.Max,
                product.Document.Type,
                product.Document.Rfisc,
                product.Document.Rfic,
                product.Codes.ServiceTypeCode,
                product.Codes.GroupCode,
                product.Codes.SubGroupCode,
                product.Codes.Description1Code,
                product.Codes.Description2Code,
                product.Terms.Refundable,
                product.Terms.Commissionable,
                product.Terms.Reusable,
                product.Terms.FormOfRefundCode,
                product.Terms.InterlineSettlementAllowed,
                product.InventoryControl,
                product.Baggage?.Pieces,
                product.Baggage?.Weight,
                product.Baggage?.WeightUnit,
                product.Status,
                product.CreatedAt,
                product.ActivatedAt,
                product.RetiredAt);
    }
}
