using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionForm.Backoffice
{
    public sealed class BackofficeGetAncillaryProvisionFormQueryHandler
        : IRequestHandler<BackofficeGetAncillaryProvisionFormQuery, BackofficeProvisionFormDto>
    {
        private readonly IGetAncillaryProvisionFormService _service;

        public BackofficeGetAncillaryProvisionFormQueryHandler(IGetAncillaryProvisionFormService service) => _service = service;

        public Task<BackofficeProvisionFormDto> Handle(BackofficeGetAncillaryProvisionFormQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ProvisionId, cancellationToken);
    }
}
