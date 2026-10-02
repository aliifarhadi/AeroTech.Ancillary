using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodeById.Backoffice
{
    public sealed record BackofficeGetServiceSubCodeByIdQuery(long ServiceSubCodeId) : IRequest<BackofficeServiceSubCodeDto>;
}
