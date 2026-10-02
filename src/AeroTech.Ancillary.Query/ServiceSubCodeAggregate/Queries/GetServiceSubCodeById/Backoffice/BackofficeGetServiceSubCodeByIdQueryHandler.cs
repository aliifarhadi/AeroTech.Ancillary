using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodeById.Backoffice
{
    public sealed class BackofficeGetServiceSubCodeByIdQueryHandler : IRequestHandler<BackofficeGetServiceSubCodeByIdQuery, BackofficeServiceSubCodeDto>
    {
        private readonly IGetServiceSubCodeByIdService _service;

        public BackofficeGetServiceSubCodeByIdQueryHandler(IGetServiceSubCodeByIdService service) => _service = service;

        public Task<BackofficeServiceSubCodeDto> Handle(BackofficeGetServiceSubCodeByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceSubCodeId, cancellationToken);
    }
}
