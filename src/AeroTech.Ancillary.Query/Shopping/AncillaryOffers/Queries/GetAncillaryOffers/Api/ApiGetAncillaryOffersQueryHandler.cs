using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public sealed class ApiGetAncillaryOffersQueryHandler : IRequestHandler<ApiGetAncillaryOffersQuery, AncillaryOffersDto>
    {
        private const string Surface = "Api";

        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetAncillaryOffersService _service;

        public ApiGetAncillaryOffersQueryHandler(IApiSalesScopeResolver salesScope, IGetAncillaryOffersService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<AncillaryOffersDto> Handle(ApiGetAncillaryOffersQuery query, CancellationToken cancellationToken)
            => AncillaryOffersMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken), Surface);
    }
}
