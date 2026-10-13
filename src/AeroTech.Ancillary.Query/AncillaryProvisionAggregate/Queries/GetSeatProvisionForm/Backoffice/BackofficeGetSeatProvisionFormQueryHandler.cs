using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionForm.Backoffice
{
    public sealed class BackofficeGetSeatProvisionFormQueryHandler
        : IRequestHandler<BackofficeGetSeatProvisionFormQuery, BackofficeSeatProvisionFormDto>
    {
        private readonly IGetSeatProvisionFormService _service;

        public BackofficeGetSeatProvisionFormQueryHandler(IGetSeatProvisionFormService service) => _service = service;

        public Task<BackofficeSeatProvisionFormDto> Handle(BackofficeGetSeatProvisionFormQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceDefinitionId, query.ProvisionId, cancellationToken);
    }
}
