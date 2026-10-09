using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto
{
    public sealed class InventoryPolicyPaginatedRowDto
    {
        public string Id { get; set; } = null!;

        [Grid("Airline")] public int OwnerAirlineId { get; set; }

        [Grid("Service")] public string ServiceDefinitionRef { get; set; } = null!;

        [Grid("Authority")] public EnumValueDto Authority { get; set; } = null!;

        [Grid("Pattern")] public EnumValueDto? LocalPattern { get; set; }

        [Grid("Provider Key")] public string? ProviderKey { get; set; }

        [Grid("Status")] public EnumValueDto Status { get; set; } = null!;

        [Grid("Version")] public long Version { get; set; }

        [Grid("Updated")] public DateTimeOffset UpdatedAt { get; set; }
    }
}
