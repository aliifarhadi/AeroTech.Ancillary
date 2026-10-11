using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Api
{
    public sealed class ApiEvaluateSelectionQueryHandler : IRequestHandler<ApiEvaluateSelectionQuery, ApiSelectionResultDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IEvaluateSelectionService _service;

        public ApiEvaluateSelectionQueryHandler(IApiSalesScopeResolver salesScope, IEvaluateSelectionService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<ApiSelectionResultDto> Handle(ApiEvaluateSelectionQuery query, CancellationToken cancellationToken)
            => ApiSelectionResultMapper.Map(
                await _service.ExecuteAsync(
                    query,
                    ApiSelectionInputMapper.ToSelection(query.Selection),
                    await _salesScope.ResolveAsync(cancellationToken),
                    cancellationToken));
    }
}
