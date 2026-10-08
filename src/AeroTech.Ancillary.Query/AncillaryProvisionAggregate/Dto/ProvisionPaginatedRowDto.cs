using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto
{
    public sealed class ProvisionPaginatedRowDto
    {
        public string Id { get; set; } = null!;

        public string ServiceDefinitionId { get; set; } = null!;

        [Grid("Sequence")] public int Sequence { get; set; }

        [Grid("Coverage")] public EnumValueDto CoverageScope { get; set; } = null!;

        [Grid("Disposition")] public EnumValueDto Disposition { get; set; } = null!;

        [Grid("Unit")] public EnumValueDto QuantityUnit { get; set; } = null!;

        [Grid("Min")] public int MinQuantity { get; set; }

        [Grid("Max")] public int MaxQuantity { get; set; }

        [Grid("Dates")] public int TravelDateCount { get; set; }

        [Grid("Seasons")] public int SeasonalPeriodCount { get; set; }

        [Grid("Blackouts")] public int BlackoutPeriodCount { get; set; }

        [Grid("Day/Time")] public int DayTimeRestrictionCount { get; set; }

        [Grid("Sales From")] public DateTimeOffset? SalesEffectiveFrom { get; set; }

        [Grid("Sales Until")] public DateTimeOffset? SalesDiscontinueAt { get; set; }

        [Grid("Status")] public EnumValueDto Status { get; set; } = null!;

        [Grid("Created")] public DateTimeOffset CreatedAt { get; set; }
    }
}
