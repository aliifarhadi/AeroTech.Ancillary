using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions.Backoffice
{
    public sealed record BackofficeGetSelectionOptionsQuery(string Family, string Reference) : IRequest<SelectionOptionsDto>, ISelectionOptionsQuery;
}
