using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductById.Backoffice
{
    public sealed record BackofficeGetAncillaryProductByIdQuery(long AncillaryProductId) : IRequest<BackofficeAncillaryProductDto>;
}
