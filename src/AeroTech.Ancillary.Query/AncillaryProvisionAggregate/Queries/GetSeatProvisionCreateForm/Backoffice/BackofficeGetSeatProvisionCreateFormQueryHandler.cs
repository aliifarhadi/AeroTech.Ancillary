using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionCreateForm.Backoffice
{
    public sealed class BackofficeGetSeatProvisionCreateFormQueryHandler
        : IRequestHandler<BackofficeGetSeatProvisionCreateFormQuery, BackofficeSeatProvisionCreateFormDto>
    {
        private readonly IGetSeatProvisionCreateFormService _service;

        public BackofficeGetSeatProvisionCreateFormQueryHandler(IGetSeatProvisionCreateFormService service) => _service = service;

        public Task<BackofficeSeatProvisionCreateFormDto> Handle(BackofficeGetSeatProvisionCreateFormQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceDefinitionId, cancellationToken);
    }
}
