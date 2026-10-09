using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice
{
    public sealed class BackofficeGetAncillaryServiceDefinitionsPaginatedQuery
        : PaginationQuery, IRequest<GridData<ServiceDefinitionPaginatedRowDto>>, IAncillaryServiceDefinitionsPaginatedQuery
    {
        public int? OwnerAirlineId { get; set; }
        public long? SupplierId { get; set; }
        public string? ServiceDefinitionRef { get; set; }
        public string? ServiceSubCode { get; set; }
        public string? ServiceTypeCode { get; set; }
        public string? GroupCode { get; set; }
        public ServiceDefinitionStatus? Status { get; set; }

        public AncillaryProfile? Profile { get; set; }

        public string? VariantCode { get; set; }
        public string? Search { get; set; }
    }
}
