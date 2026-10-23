using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Options;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions
{
    public interface IGetSelectionOptionsService
    {
        Task<SelectionOptionsForm> ExecuteAsync(ISelectionOptionsQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
