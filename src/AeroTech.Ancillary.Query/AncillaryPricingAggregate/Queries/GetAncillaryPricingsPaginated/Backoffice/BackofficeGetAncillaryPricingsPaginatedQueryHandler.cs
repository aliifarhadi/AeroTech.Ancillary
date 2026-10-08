using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated.Backoffice
{
    public sealed class BackofficeGetAncillaryPricingsPaginatedQueryHandler
        : IRequestHandler<BackofficeGetAncillaryPricingsPaginatedQuery, GridData<PricingPaginatedRowDto>>
    {
        private readonly IGetAncillaryPricingsPaginatedService _service;

        public BackofficeGetAncillaryPricingsPaginatedQueryHandler(IGetAncillaryPricingsPaginatedService service)
            => _service = service;

        public Task<GridData<PricingPaginatedRowDto>> Handle(
            BackofficeGetAncillaryPricingsPaginatedQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
