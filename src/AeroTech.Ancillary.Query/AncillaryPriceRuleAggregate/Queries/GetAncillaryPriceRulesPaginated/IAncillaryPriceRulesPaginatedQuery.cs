using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRulesPaginated
{
    public interface IAncillaryPriceRulesPaginatedQuery
    {
        int? OwnerAirlineId { get; }

        string? ProductRef { get; }

        int? CurrencyId { get; }

        AncillaryPriceRuleStatus? Status { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
