using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions.OtaPanel
{
    public sealed record OtaPanelGetSelectionOptionsQuery(string Family, string Reference, string? OfferId) : IRequest<SelectionOptionsDto>, ISelectionOptionsQuery;
}
