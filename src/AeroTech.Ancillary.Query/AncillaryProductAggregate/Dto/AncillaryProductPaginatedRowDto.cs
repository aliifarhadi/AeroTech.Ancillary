using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto
{
    public sealed class AncillaryProductPaginatedRowDto
    {
        public string Id { get; set; } = null!;

        [Grid("Airline")] public int OwnerAirlineId { get; set; }

        [Grid("Product")] public string ProductRef { get; set; } = null!;

        [Grid("Ver.")] public int Version { get; set; }

        [Grid("Type")] public EnumValueDto Type { get; set; } = null!;

        [Grid("Name")] public string Name { get; set; } = null!;
        public string? Description { get; set; }

        [Grid("Sales Scope")] public EnumValueDto SalesScope { get; set; } = null!;
        public ProductQuantityDto Quantity { get; set; } = null!;
        public ProductDocumentDto Document { get; set; } = null!;
        public ProductCodesDto Codes { get; set; } = null!;
        public ProductTermsDto Terms { get; set; } = null!;

        [Grid("Inventory")] public EnumValueDto InventoryControl { get; set; } = null!;
        public ProductBaggageDto? Baggage { get; set; }
        public ProductLoungeDto? Lounge { get; set; }

        [Grid("Status")] public EnumValueDto Status { get; set; } = null!;

        [Grid("Created")] public DateTimeOffset CreatedAt { get; set; }

        [Grid("Activated")] public DateTimeOffset? ActivatedAt { get; set; }

        [Grid("Retired")] public DateTimeOffset? RetiredAt { get; set; }
    }
}
