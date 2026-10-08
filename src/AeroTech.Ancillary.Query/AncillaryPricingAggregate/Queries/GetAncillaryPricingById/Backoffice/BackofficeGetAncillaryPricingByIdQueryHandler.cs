using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById.Backoffice
{
    public sealed class BackofficeGetAncillaryPricingByIdQueryHandler
        : IRequestHandler<BackofficeGetAncillaryPricingByIdQuery, BackofficePricingDto>
    {
        private readonly IGetAncillaryPricingByIdService _service;

        public BackofficeGetAncillaryPricingByIdQueryHandler(IGetAncillaryPricingByIdService service) => _service = service;

        public Task<BackofficePricingDto> Handle(BackofficeGetAncillaryPricingByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.PricingId, cancellationToken);
    }
}
