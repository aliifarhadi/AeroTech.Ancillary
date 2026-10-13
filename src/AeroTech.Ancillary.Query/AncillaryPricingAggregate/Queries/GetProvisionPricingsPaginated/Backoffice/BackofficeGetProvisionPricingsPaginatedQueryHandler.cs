using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetProvisionPricingsPaginated.Backoffice
{
    public sealed class BackofficeGetProvisionPricingsPaginatedQueryHandler
        : IRequestHandler<BackofficeGetProvisionPricingsPaginatedQuery, GridData<ProvisionPricingRowDto>>
    {
        private readonly IGetProvisionPricingsPaginatedService _service;

        public BackofficeGetProvisionPricingsPaginatedQueryHandler(IGetProvisionPricingsPaginatedService service) => _service = service;

        public Task<GridData<ProvisionPricingRowDto>> Handle(BackofficeGetProvisionPricingsPaginatedQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
