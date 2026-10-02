using System.Globalization;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductById
{
    public static class AncillaryProductMapper
    {
        public static BackofficeAncillaryProductDto ToBackofficeProduct(AncillaryProductReadModel product)
            => new(
                product.Id,
                product.OwnerAirlineId,
                product.ProductRef,
                product.Version,
                EnumValueDto.Of(product.Type),
                product.Name,
                product.Description,
                EnumValueDto.Of(product.SalesScope),
                ToQuantity(product),
                ToDocument(product),
                ToCodes(product),
                ToTerms(product),
                EnumValueDto.Of(product.InventoryControl),
                ToBaggage(product),
                EnumValueDto.Of(product.Status),
                product.CreatedAt,
                product.ActivatedAt,
                product.RetiredAt);

        public static AncillaryProductPaginatedRowDto ToPaginatedRow(AncillaryProductReadModel product)
            => new()
            {
                Id = product.Id.ToString(CultureInfo.InvariantCulture),
                OwnerAirlineId = product.OwnerAirlineId,
                ProductRef = product.ProductRef,
                Version = product.Version,
                Type = EnumValueDto.Of(product.Type),
                Name = product.Name,
                Description = product.Description,
                SalesScope = EnumValueDto.Of(product.SalesScope),
                Quantity = ToQuantity(product),
                Document = ToDocument(product),
                Codes = ToCodes(product),
                Terms = ToTerms(product),
                InventoryControl = EnumValueDto.Of(product.InventoryControl),
                Baggage = ToBaggage(product),
                Status = EnumValueDto.Of(product.Status),
                CreatedAt = product.CreatedAt,
                ActivatedAt = product.ActivatedAt,
                RetiredAt = product.RetiredAt
            };

        private static ProductQuantityDto ToQuantity(AncillaryProductReadModel product)
            => new(EnumValueDto.Of(product.QuantityUnit), product.QuantityMin, product.QuantityMax);

        private static ProductDocumentDto ToDocument(AncillaryProductReadModel product)
            => new(EnumValueDto.Of(product.DocumentType), product.Rfisc, product.Rfic);

        private static ProductCodesDto ToCodes(AncillaryProductReadModel product)
            => new(
                product.ServiceTypeCode,
                product.GroupCode,
                product.SubGroupCode,
                product.Description1Code,
                product.Description2Code);

        private static ProductTermsDto ToTerms(AncillaryProductReadModel product)
            => new(
                product.Refundable,
                product.Commissionable,
                product.Reusable,
                product.FormOfRefundCode,
                product.InterlineSettlementAllowed);

        private static ProductBaggageDto? ToBaggage(AncillaryProductReadModel product)
            => product.BaggagePieces is null && product.BaggageWeight is null && product.BaggageWeightUnit is null
                ? null
                : new ProductBaggageDto(
                    product.BaggagePieces,
                    product.BaggageWeight,
                    EnumValueDto.OfNullable(product.BaggageWeightUnit));
    }
}
