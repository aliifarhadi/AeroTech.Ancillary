using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.Api
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
            var detail = await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken);

            return ApiAncillaryOfferDetailMapper.Map(detail, await _references.ReadAsync(detail.Shopping, cancellationToken));
        }
    }
}
