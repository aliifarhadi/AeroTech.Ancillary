using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById.Backoffice
{
    public sealed class BackofficeGetAncillaryServiceDefinitionByIdQueryHandler
        : IRequestHandler<BackofficeGetAncillaryServiceDefinitionByIdQuery, BackofficeServiceDefinitionDto>
    {
        private readonly IGetAncillaryServiceDefinitionByIdService _service;

        public BackofficeGetAncillaryServiceDefinitionByIdQueryHandler(IGetAncillaryServiceDefinitionByIdService service)
            => _service = service;

        public Task<BackofficeServiceDefinitionDto> Handle(BackofficeGetAncillaryServiceDefinitionByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceDefinitionId, cancellationToken);
    }
}
