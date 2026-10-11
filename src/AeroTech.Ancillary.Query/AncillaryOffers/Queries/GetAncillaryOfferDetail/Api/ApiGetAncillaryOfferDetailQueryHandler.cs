using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.Api
{
    public sealed class ApiGetAncillaryOfferDetailQueryHandler : IRequestHandler<ApiGetAncillaryOfferDetailQuery, ApiAncillaryOfferDetailDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOfferDetailService _service;

        public ApiGetAncillaryOfferDetailQueryHandler(IApiSalesScopeResolver salesScope, IGetAncillaryOfferDetailService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<ApiAncillaryOfferDetailDto> Handle(ApiGetAncillaryOfferDetailQuery query, CancellationToken cancellationToken)
            => ApiAncillaryOfferDetailMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
