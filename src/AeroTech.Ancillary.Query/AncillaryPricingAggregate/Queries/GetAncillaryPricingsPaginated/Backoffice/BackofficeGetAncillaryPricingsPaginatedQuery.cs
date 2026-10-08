using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated.Backoffice
{
    public sealed class BackofficeGetAncillaryPricingsPaginatedQuery
        : PaginationQuery, IRequest<GridData<PricingPaginatedRowDto>>, IAncillaryPricingsPaginatedQuery
    {
        public long? AncillaryProvisionId { get; set; }

        public PricingStatus? Status { get; set; }
    }
}
