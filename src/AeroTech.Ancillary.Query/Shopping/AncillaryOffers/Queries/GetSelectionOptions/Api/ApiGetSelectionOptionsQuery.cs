using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions.Api
{
    public sealed record ApiGetSelectionOptionsQuery(string Family, string Reference) : IRequest<SelectionOptionsDto>, ISelectionOptionsQuery;
}
