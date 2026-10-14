using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail.Backoffice
{
    public sealed class BackofficeGetAncillaryOfferDetailQueryHandler : IRequestHandler<BackofficeGetAncillaryOfferDetailQuery, BackofficeAncillaryOfferDetailDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOfferDetailService _service;

        public BackofficeGetAncillaryOfferDetailQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetAncillaryOfferDetailService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeAncillaryOfferDetailDto> Handle(BackofficeGetAncillaryOfferDetailQuery query, CancellationToken cancellationToken)
            => BackofficeAncillaryOfferDetailMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(query.Options, cancellationToken), OfferFamilyFilters.Services, cancellationToken));
    }
}
