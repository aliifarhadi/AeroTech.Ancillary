using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto
{
    public sealed class AncillaryPriceRulePaginatedRowDto
    {
        public string Id { get; set; } = null!;

        [Grid("Airline")] public int OwnerAirlineId { get; set; }

        [Grid("Product")] public string ProductRef { get; set; } = null!;

        [Grid("Priority")] public int Priority { get; set; }

        [Grid("Currency")] public int CurrencyId { get; set; }
        public IReadOnlyList<PriceLineDto> Lines { get; set; } = Array.Empty<PriceLineDto>();

        [Grid("Sales From")] public DateTimeOffset? SalesFrom { get; set; }

        [Grid("Sales To")] public DateTimeOffset? SalesTo { get; set; }

        [Grid("Travel From")] public DateOnly? TravelFrom { get; set; }

        [Grid("Travel To")] public DateOnly? TravelTo { get; set; }
        public PriceRuleConditionsDto Conditions { get; set; } = null!;

        [Grid("Status")] public EnumValueDto Status { get; set; } = null!;

        [Grid("Created")] public DateTimeOffset CreatedAt { get; set; }
    }
}
