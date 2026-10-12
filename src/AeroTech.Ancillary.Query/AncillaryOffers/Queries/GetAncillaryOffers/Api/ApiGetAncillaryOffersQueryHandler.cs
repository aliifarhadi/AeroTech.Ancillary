using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public sealed class ApiGetAncillaryOffersQueryHandler : IRequestHandler<ApiGetAncillaryOffersQuery, ApiAncillaryOffersDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOffersService _service;
        private readonly IApiOfferReferencesReader _references;

        public ApiGetAncillaryOffersQueryHandler(IApiSalesScopeResolver salesScope, IGetAncillaryOffersService service, IApiOfferReferencesReader references)
        {
            _salesScope = salesScope;
            _service = service;
            _references = references;
        }

        public async Task<ApiAncillaryOffersDto> Handle(ApiGetAncillaryOffersQuery query, CancellationToken cancellationToken)
        {
            var shopping = await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken);

            return ApiAncillaryOffersMapper.Map(shopping, await _references.ReadAsync(shopping, cancellationToken));
        }
    }
}
