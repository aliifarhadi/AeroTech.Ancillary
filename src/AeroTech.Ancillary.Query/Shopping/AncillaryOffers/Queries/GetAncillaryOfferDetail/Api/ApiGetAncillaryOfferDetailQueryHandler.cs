using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail.Api
{
    public sealed class ApiGetAncillaryOfferDetailQueryHandler : IRequestHandler<ApiGetAncillaryOfferDetailQuery, ApiAncillaryOfferDetailDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOfferDetailService _service;
        private readonly IApiOfferReferencesReader _references;

        public ApiGetAncillaryOfferDetailQueryHandler(
            IApiSalesScopeResolver salesScope,
            IGetAncillaryOfferDetailService service,
            IApiOfferReferencesReader references)
        {
            _salesScope = salesScope;
            _service = service;
            _references = references;
        }

        public async Task<ApiAncillaryOfferDetailDto> Handle(ApiGetAncillaryOfferDetailQuery query, CancellationToken cancellationToken)
        {
            var detail = await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), OfferFamilyFilters.Services, cancellationToken);

            return ApiAncillaryOfferDetailMapper.Map(detail, await _references.ReadAsync(detail.Shopping, cancellationToken));
        }
    }
}
