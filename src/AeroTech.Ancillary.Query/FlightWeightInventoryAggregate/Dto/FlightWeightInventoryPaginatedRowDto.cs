using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Dto
{
    public sealed class FlightWeightInventoryPaginatedRowDto
    {
        public string Id { get; set; } = null!;

        [Grid("Airline")] public int OwnerAirlineId { get; set; }

        [Grid("Flight")] public string FlightId { get; set; } = null!;

        [Grid("Weight Resource")] public string WeightResourceId { get; set; } = null!;

        [Grid("Capacity (kg)")] public decimal CapacityKg { get; set; }

        [Grid("Closed")] public bool ClosedForSale { get; set; }

        [Grid("Status")] public EnumValueDto Status { get; set; } = null!;

        [Grid("Version")] public long Version { get; set; }

        [Grid("Adjustments")] public int AdjustmentCount { get; set; }

        [Grid("Updated")] public DateTimeOffset UpdatedAt { get; set; }
    }
}
