using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodesPaginated
{
    public interface IServiceSubCodesPaginatedQuery
    {
        int? OwnerAirlineId { get; }

        string? Code { get; }

        ServiceSubCodeSource? Source { get; }

        ServiceSubCodeStatus? Status { get; }

        int PageNumber { get; }

        int PageSize { get; }
    }
}
