using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById.Backoffice
{
    public sealed class BackofficeGetAncillaryProvisionByIdQueryHandler
        : IRequestHandler<BackofficeGetAncillaryProvisionByIdQuery, BackofficeProvisionDto>
    {
        private readonly IGetAncillaryProvisionByIdService _service;

        public BackofficeGetAncillaryProvisionByIdQueryHandler(IGetAncillaryProvisionByIdService service) => _service = service;

        public Task<BackofficeProvisionDto> Handle(BackofficeGetAncillaryProvisionByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ProvisionId, cancellationToken);
    }
}
