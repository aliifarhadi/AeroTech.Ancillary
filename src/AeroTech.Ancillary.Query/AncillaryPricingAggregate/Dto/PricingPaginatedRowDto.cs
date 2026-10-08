using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto
{
    public sealed class PricingPaginatedRowDto
    {
        public string Id { get; set; } = null!;

        public string AncillaryProvisionId { get; set; } = null!;

        [Grid("Version")] public int Version { get; set; }

        [Grid("Pricing Unit")] public EnumValueDto? PricingUnit { get; set; }

        [Grid("Currency")] public string? Currency { get; set; }

        [Grid("Applies Per")] public EnumValueDto? FeeApplicationUnit { get; set; }

        [Grid("Rates")] public int RateCount { get; set; }

        [Grid("Status")] public EnumValueDto Status { get; set; } = null!;

        [Grid("Created")] public DateTimeOffset CreatedAt { get; set; }

        [Grid("Activated")] public DateTimeOffset? ActivatedAt { get; set; }
    }
}
