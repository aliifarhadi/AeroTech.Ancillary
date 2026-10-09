using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Dto
{
    public sealed class FlightCountInventoryPaginatedRowDto
    {
        public string Id { get; set; } = null!;

        [Grid("Airline")] public int OwnerAirlineId { get; set; }

        [Grid("Flight")] public string FlightId { get; set; } = null!;

        [Grid("Resource")] public string ResourceId { get; set; } = null!;

        [Grid("Unit")] public EnumValueDto CountUnit { get; set; } = null!;

        [Grid("Total")] public int TotalCapacity { get; set; }

        [Grid("Closed")] public bool ClosedForSale { get; set; }

        [Grid("Status")] public EnumValueDto Status { get; set; } = null!;

        [Grid("Version")] public long Version { get; set; }

        [Grid("Adjustments")] public int AdjustmentCount { get; set; }

        [Grid("Updated")] public DateTimeOffset UpdatedAt { get; set; }
    }
}
