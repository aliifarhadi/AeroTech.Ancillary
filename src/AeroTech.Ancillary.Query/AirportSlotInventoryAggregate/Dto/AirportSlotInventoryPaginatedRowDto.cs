using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Dto
{
    public sealed class AirportSlotInventoryPaginatedRowDto
    {
        public string Id { get; set; } = null!;

        [Grid("Airline")] public int OwnerAirlineId { get; set; }

        [Grid("Airport")] public int AirportId { get; set; }

        [Grid("Facility")] public string FacilityId { get; set; } = null!;

        [Grid("Start (UTC)")] public DateTimeOffset StartUtc { get; set; }

        [Grid("End (UTC)")] public DateTimeOffset EndUtc { get; set; }

        [Grid("Persons")] public int CapacityPersons { get; set; }

        [Grid("Closed")] public bool ClosedForSale { get; set; }

        [Grid("Status")] public EnumValueDto Status { get; set; } = null!;

        [Grid("Version")] public long Version { get; set; }

        [Grid("Adjustments")] public int AdjustmentCount { get; set; }

        [Grid("Updated")] public DateTimeOffset UpdatedAt { get; set; }
    }
}
