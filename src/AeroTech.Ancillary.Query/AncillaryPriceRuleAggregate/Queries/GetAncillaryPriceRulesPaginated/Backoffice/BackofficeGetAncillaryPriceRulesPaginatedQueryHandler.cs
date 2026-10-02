using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRulesPaginated.Backoffice
{
    public sealed class BackofficeGetAncillaryPriceRulesPaginatedQueryHandler
        : IRequestHandler<BackofficeGetAncillaryPriceRulesPaginatedQuery, GridData<AncillaryPriceRulePaginatedRowDto>>
    {
        private readonly IGetAncillaryPriceRulesPaginatedService _service;

        public BackofficeGetAncillaryPriceRulesPaginatedQueryHandler(IGetAncillaryPriceRulesPaginatedService service) => _service = service;

        public Task<GridData<AncillaryPriceRulePaginatedRowDto>> Handle(
            BackofficeGetAncillaryPriceRulesPaginatedQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
