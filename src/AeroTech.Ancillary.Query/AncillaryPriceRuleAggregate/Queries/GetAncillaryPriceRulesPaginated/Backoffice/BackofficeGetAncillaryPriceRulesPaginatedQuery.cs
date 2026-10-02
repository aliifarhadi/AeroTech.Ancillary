using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRulesPaginated.Backoffice
{
    public sealed class BackofficeGetAncillaryPriceRulesPaginatedQuery
        : PaginationQuery, IRequest<GridData<AncillaryPriceRulePaginatedRowDto>>, IAncillaryPriceRulesPaginatedQuery
    {
        public int? OwnerAirlineId { get; set; }
        public string? ProductRef { get; set; }
        public int? CurrencyId { get; set; }
        public AncillaryPriceRuleStatus? Status { get; set; }
    }
}
