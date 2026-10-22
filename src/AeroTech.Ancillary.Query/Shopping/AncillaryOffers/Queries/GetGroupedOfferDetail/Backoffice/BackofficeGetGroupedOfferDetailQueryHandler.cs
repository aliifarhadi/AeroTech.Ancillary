using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Backoffice;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail.Backoffice
{
    public sealed class BackofficeGetGroupedOfferDetailQueryHandler : IRequestHandler<BackofficeGetGroupedOfferDetailQuery, BackofficeGroupedOfferDetailDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetGroupedOfferDetailService _service;

        public BackofficeGetGroupedOfferDetailQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetGroupedOfferDetailService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeGroupedOfferDetailDto> Handle(BackofficeGetGroupedOfferDetailQuery query, CancellationToken cancellationToken)
            => BackofficeGroupedOfferDetailMapper.Map(
                await _service.ExecuteAsync(
                    query,
                    query.Family,
                    query.Selection is null ? null : BackofficeSelectionInputMapper.ToSelection(query.Selection),
                    await _salesScope.ResolveAsync(query.Options, cancellationToken),
                    cancellationToken));
    }
}
