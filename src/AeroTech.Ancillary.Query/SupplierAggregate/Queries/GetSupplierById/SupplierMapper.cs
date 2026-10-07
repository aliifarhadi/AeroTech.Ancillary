using System.Globalization;
using AeroTech.Ancillary.Query.SupplierAggregate.Dto;
using AeroTech.Ancillary.Query.SupplierAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSupplierById
{
    public static class SupplierMapper
    {
        public static BackofficeSupplierDto ToBackofficeSupplier(SupplierReadModel supplier)
            => new(
                supplier.Id,
                supplier.OwnerAirlineId,
                supplier.Name,
                EnumValueDto.Of(supplier.FulfillmentKind),
                supplier.FulfillmentProviderKey,
                EnumValueDto.Of(supplier.Status),
                supplier.CreatedAt,
                supplier.RetiredAt);

        public static SupplierPaginatedRowDto ToPaginatedRow(SupplierReadModel supplier)
            => new()
            {
                Id = supplier.Id.ToString(CultureInfo.InvariantCulture),
                OwnerAirlineId = supplier.OwnerAirlineId,
                Name = supplier.Name,
                FulfillmentKind = EnumValueDto.Of(supplier.FulfillmentKind),
                FulfillmentProviderKey = supplier.FulfillmentProviderKey,
                Status = EnumValueDto.Of(supplier.Status),
                CreatedAt = supplier.CreatedAt
            };
    }
}
