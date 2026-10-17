using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionCreateForm.Backoffice
{
    public sealed class BackofficeGetProfileProvisionCreateFormQueryHandler
        : IRequestHandler<BackofficeGetProfileProvisionCreateFormQuery, BackofficeProfileProvisionCreateFormDto>
    {
        private readonly IGetProfileProvisionCreateFormService _service;

        public BackofficeGetProfileProvisionCreateFormQueryHandler(IGetProfileProvisionCreateFormService service) => _service = service;

        public Task<BackofficeProfileProvisionCreateFormDto> Handle(BackofficeGetProfileProvisionCreateFormQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.Profile, query.ServiceDefinitionId, cancellationToken);
    }
}
