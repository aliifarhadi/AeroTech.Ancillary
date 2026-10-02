using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductById.Backoffice
{
    public sealed class BackofficeGetAncillaryProductByIdQueryHandler : IRequestHandler<BackofficeGetAncillaryProductByIdQuery, BackofficeAncillaryProductDto>
    {
        private readonly IGetAncillaryProductByIdService _service;

        public BackofficeGetAncillaryProductByIdQueryHandler(IGetAncillaryProductByIdService service) => _service = service;

        public Task<BackofficeAncillaryProductDto> Handle(BackofficeGetAncillaryProductByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.AncillaryProductId, cancellationToken);
    }
}
