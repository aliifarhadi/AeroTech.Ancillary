using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public sealed class ApiGetAncillaryOffersQueryHandler : IRequestHandler<ApiGetAncillaryOffersQuery, ApiAncillaryOffersDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOffersService _service;

        public ApiGetAncillaryOffersQueryHandler(IApiSalesScopeResolver salesScope, IGetAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<ApiAncillaryOffersDto> Handle(ApiGetAncillaryOffersQuery query, CancellationToken cancellationToken)
            => ApiAncillaryOffersMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
