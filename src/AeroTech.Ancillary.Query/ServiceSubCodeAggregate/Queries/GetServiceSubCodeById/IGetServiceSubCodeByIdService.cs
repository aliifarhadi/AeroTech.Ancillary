using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodeById
{
    public interface IGetServiceSubCodeByIdService
    {
        Task<BackofficeServiceSubCodeDto> ExecuteAsync(long serviceSubCodeId, CancellationToken cancellationToken = default);
    }
}
