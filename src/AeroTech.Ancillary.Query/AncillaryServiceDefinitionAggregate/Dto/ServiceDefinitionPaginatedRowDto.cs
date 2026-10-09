using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    public sealed class ServiceDefinitionPaginatedRowDto
    {
        public string Id { get; set; } = null!;

        [Grid("Airline")] public int OwnerAirlineId { get; set; }

        [Grid("Reference")] public string ServiceDefinitionRef { get; set; } = null!;

        [Grid("Ver.")] public int Version { get; set; }

        [Grid("Name")] public string CommercialName { get; set; } = null!;

        [Grid("Sub Code")] public string ServiceSubCode { get; set; } = null!;

        [Grid("Source")] public EnumValueDto SubCodeSource { get; set; } = null!;

        [Grid("Type")] public string ServiceTypeCode { get; set; } = null!;

        [Grid("Group")] public string GroupCode { get; set; } = null!;

        [Grid("Pricing Unit")] public EnumValueDto? PricingUnit { get; set; }

        [Grid("Service Date Basis")] public EnumValueDto? ServiceDateBasis { get; set; }

        [Grid("Profile")] public EnumValueDto? Profile { get; set; }

        [Grid("Variant")] public string? VariantCode { get; set; }

        public string SupplierId { get; set; } = null!;

        [Grid("Supplier")] public string SupplierName { get; set; } = null!;

        [Grid("Document")] public EnumValueDto DocumentType { get; set; } = null!;

        [Grid("Status")] public EnumValueDto Status { get; set; } = null!;

        [Grid("Created")] public DateTimeOffset CreatedAt { get; set; }
    }
}
