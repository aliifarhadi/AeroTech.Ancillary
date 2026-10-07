using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Dto
{
    public sealed class SupplierPaginatedRowDto
    {
        public string Id { get; set; } = null!;

        [Grid("Airline")] public int OwnerAirlineId { get; set; }

        [Grid("Name")] public string Name { get; set; } = null!;

        [Grid("Fulfillment")] public EnumValueDto FulfillmentKind { get; set; } = null!;

        [Grid("Provider Key")] public string? FulfillmentProviderKey { get; set; }

        [Grid("Status")] public EnumValueDto Status { get; set; } = null!;

        [Grid("Created")] public DateTimeOffset CreatedAt { get; set; }
    }
}
